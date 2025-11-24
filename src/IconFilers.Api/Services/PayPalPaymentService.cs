using IconFilers.Api.IServices;
using IconFilers.Application;
using IconFilers.Application.DTOs;
using IconFilers.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace IconFilers.Api.Services;

public class PayPalPaymentService : IPaymentService
{
    private readonly HttpClient _http;
    private readonly PayPalOptions _options;
    private readonly AppDbContext _db;
    private readonly ILogger<PayPalPaymentService> _logger;
    private readonly string _baseUrl;
    // In-memory idempotency cache: Idempotency-Key -> PaymentId
    private static readonly ConcurrentDictionary<string, int> _idempotencyCache = new();

    public PayPalPaymentService(HttpClient http, IOptions<PayPalOptions> opts, AppDbContext db, ILogger<PayPalPaymentService> logger)
    {
        _http = http;
        _options = opts.Value;
        _db = db;
        _logger = logger;
        _baseUrl = _options.UseSandbox ? "https://api-m.sandbox.paypal.com" : "https://api-m.paypal.com";
    }

    private async Task<string> GetAccessTokenAsync()
    {
        var tokenUrl = $"{_baseUrl}/v1/oauth2/token";
        var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.ClientId}:{_options.Secret}"));

        using var req = new HttpRequestMessage(HttpMethod.Post, tokenUrl);
        req.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);
        req.Content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");

        var res = await _http.SendAsync(req);
        var body = await res.Content.ReadAsStringAsync();
        if (!res.IsSuccessStatusCode)
        {
            _logger.LogError("PayPal token error: {Status} {Body}", res.StatusCode, body);
            throw new Exception($"PayPal token request failed: {res.StatusCode}");
        }

        using var doc = JsonDocument.Parse(body);
        return doc.RootElement.GetProperty("access_token").GetString() ?? throw new Exception("No access_token in PayPal response");
    }

    public async Task<CreatePaymentResponseDto> CreatePaymentAsync(CreatePaymentRequestDto request, string? idempotencyKey = null)
    {
        // Basic validation
        if (request.Amount < 0 || request.TaxAmount < 0 || request.Discount < 0)
            throw new ArgumentException("Amounts must be non-negative", nameof(request));
        if (string.IsNullOrWhiteSpace(request.Currency)) throw new ArgumentException("Currency is required", nameof(request));
        if (string.IsNullOrWhiteSpace(request.ReturnUrl)) throw new ArgumentException("ReturnUrl is required", nameof(request));
        if (string.IsNullOrWhiteSpace(request.CancelUrl)) throw new ArgumentException("CancelUrl is required", nameof(request));
        if (string.IsNullOrWhiteSpace(_options.ClientId) || string.IsNullOrWhiteSpace(_options.Secret))
            throw new InvalidOperationException("PayPal credentials are not configured.");

        // compute safe amounts
        decimal itemTotal = decimal.Round(request.Amount - request.Discount, 2, MidpointRounding.AwayFromZero);
        decimal taxTotal = decimal.Round(request.TaxAmount, 2, MidpointRounding.AwayFromZero);
        decimal netAmount = decimal.Round(itemTotal + taxTotal, 2, MidpointRounding.AwayFromZero);

        // idempotency quick check (in-memory cache approach already in service)
        if (!string.IsNullOrWhiteSpace(idempotencyKey) && _idempotencyCache.TryGetValue(idempotencyKey, out var existingId))
        {
            var existing = await _db.Payments.FindAsync(existingId);
            if (existing != null)
            {
                _logger.LogInformation("Idempotent request returning existing payment. Key:{Key} PaymentId:{PaymentId}", idempotencyKey, existingId);
                return new CreatePaymentResponseDto { PaymentId = existing.Id, OrderId = "", ApproveUrl = "" };
            }
        }

        var payment = new Payment
        {
            ClientId = request.ClientId,
            Amount = request.Amount,
            TaxAmount = request.TaxAmount,
            Discount = request.Discount,
            NetAmount = netAmount,
            PaymentMode = "PayPal",
            Status = "CREATED",
            CreatedAt = DateTime.UtcNow
        };

        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            // build PayPal order payload
            var body = new
            {
                intent = "CAPTURE",
                purchase_units = new[]
                {
                new {
                    amount = new {
                        currency_code = request.Currency,
                        value = netAmount.ToString("0.00", CultureInfo.InvariantCulture),
                        breakdown = new {
                            item_total = new { currency_code = request.Currency, value = itemTotal.ToString("0.00", CultureInfo.InvariantCulture) },
                            tax_total  = new { currency_code = request.Currency, value = taxTotal.ToString("0.00", CultureInfo.InvariantCulture) }
                        }
                    },
                    description = $"Payment for client {request.ClientId} (paymentId:{payment.Id})"
                }
            },
                application_context = new { return_url = request.ReturnUrl, cancel_url = request.CancelUrl }
            };

            var token = await GetAccessTokenAsync(); // make sure this throws descriptive error if it fails

            var url = $"{_baseUrl}/v2/checkout/orders";
            using var reqMsg = new HttpRequestMessage(HttpMethod.Post, url);
            reqMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            if (!string.IsNullOrEmpty(idempotencyKey)) reqMsg.Headers.Add("PayPal-Request-Id", idempotencyKey);
            reqMsg.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var res = await _http.SendAsync(reqMsg);
            var responseBody = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
            {
                // Log full response for troubleshooting
                _logger.LogError("PayPal create order failed. Status:{Status} PaymentId:{PaymentId} Body:{Body}", res.StatusCode, payment.Id, responseBody);
                // mark DB row for failure and rollback
                payment.Status = "ORDER_CREATE_FAILED";
                await _db.SaveChangesAsync();
                await tx.RollbackAsync();
                throw new Exception($"PayPal create order failed: {res.StatusCode}");
            }

            // parse JSON defensively
            JsonDocument doc;
            try
            {
                doc = JsonDocument.Parse(responseBody);
            }
            catch (Exception parseEx)
            {
                _logger.LogError(parseEx, "Failed to parse PayPal create order response. PaymentId:{PaymentId} Body:{Body}", payment.Id, responseBody);
                payment.Status = "ORDER_CREATE_PARSE_FAILED";
                await _db.SaveChangesAsync();
                await tx.RollbackAsync();
                throw;
            }

            var root = doc.RootElement;

            string orderId = root.TryGetProperty("id", out var idProp) ? idProp.GetString() ?? "" : "";
            string approveUrl = "";

            if (root.TryGetProperty("links", out var links))
            {
                foreach (var l in links.EnumerateArray())
                {
                    if (l.TryGetProperty("rel", out var rel) && rel.GetString() == "approve" && l.TryGetProperty("href", out var href))
                    {
                        approveUrl = href.GetString() ?? "";
                        break;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(orderId))
            {
                _logger.LogError("PayPal response missing order id. PaymentId:{PaymentId} Body:{Body}", payment.Id, responseBody);
                payment.Status = "ORDER_CREATE_NO_ID";
                await _db.SaveChangesAsync();
                await tx.RollbackAsync();
                throw new Exception("PayPal response missing order id");
            }

            // store orderId in the DB to avoid orphaned orders (we don't change schema; store temporarily in Status)
            payment.Status = $"ORDER_CREATED:{orderId}";
            await _db.SaveChangesAsync();

            // track idempotency in-memory
            if (!string.IsNullOrWhiteSpace(idempotencyKey))
                _idempotencyCache[idempotencyKey] = payment.Id;

            await tx.CommitAsync();

            return new CreatePaymentResponseDto
            {
                PaymentId = payment.Id,
                OrderId = orderId,
                ApproveUrl = approveUrl
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreatePaymentAsync failed. ClientId:{ClientId} PaymentId:{PaymentId}", request.ClientId, payment.Id);
            try { await tx.RollbackAsync(); } catch { /* ignore */ }
            throw;
        }
    }


    public async Task<CapturePaymentResponseDto> CapturePaymentAsync(CapturePaymentRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.OrderId))
            throw new ArgumentException("OrderId is required");

        var payment = await _db.Payments.FirstOrDefaultAsync(p => p.Id == request.PaymentId);
        if (payment == null)
        {
            _logger.LogWarning("Capture failed: Payment not found. PaymentId:{PaymentId} OrderId:{OrderId}", request.PaymentId, request.OrderId);
            throw new KeyNotFoundException("Payment not found");
        }

        if (payment.Status == "COMPLETED")
        {
            _logger.LogInformation("Capture called on already completed payment. PaymentId:{PaymentId} OrderId:{OrderId}", payment.Id, request.OrderId);
            return new CapturePaymentResponseDto { Status = "ALREADY_COMPLETED", CapturedAmount = payment.NetAmount, Currency = "" };
        }

        // Only allow capture if status is ORDER_CREATED:{orderId}
        if (payment.Status == null || !payment.Status.StartsWith("ORDER_CREATED:"))
        {
            _logger.LogWarning("Invalid payment status for capture. PaymentId:{PaymentId} Status:{Status} OrderId:{OrderId}", payment.Id, payment.Status, request.OrderId);
            throw new InvalidOperationException("Payment is not in a capturable state");
        }

        // Optionally, check that orderId matches
        var storedOrderId = payment.Status.Substring("ORDER_CREATED:".Length);
        if (!string.Equals(storedOrderId, request.OrderId, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("OrderId mismatch on capture. PaymentId:{PaymentId} ExpectedOrderId:{ExpectedOrderId} ProvidedOrderId:{ProvidedOrderId}", payment.Id, storedOrderId, request.OrderId);
            throw new InvalidOperationException("OrderId does not match the payment record");
        }

        var token = await GetAccessTokenAsync();
        var url = $"{_baseUrl}/v2/checkout/orders/{request.OrderId}/capture";

        using var reqMsg = new HttpRequestMessage(HttpMethod.Post, url);
        reqMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        reqMsg.Content = new StringContent("{}", Encoding.UTF8, "application/json");

        var res = await _http.SendAsync(reqMsg);
        var json = await res.Content.ReadAsStringAsync();
        if (!res.IsSuccessStatusCode)
        {
            _logger.LogError("PayPal capture failed: {Status} {Body} PaymentId:{PaymentId} OrderId:{OrderId}", res.StatusCode, json, payment.Id, request.OrderId);
            payment.Status = "CAPTURE_FAILED";
            await _db.SaveChangesAsync();
            throw new Exception($"PayPal capture failed: {res.StatusCode}");
        }

        using var doc = JsonDocument.Parse(json);
        decimal capturedAmount = 0m;
        string currency = "";

        if (doc.RootElement.TryGetProperty("purchase_units", out var puArr) && puArr.GetArrayLength() > 0)
        {
            var pu = puArr[0];
            if (pu.TryGetProperty("payments", out var payments) && payments.TryGetProperty("captures", out var caps) && caps.GetArrayLength() > 0)
            {
                var cap = caps[0];
                var amt = cap.GetProperty("amount");
                var valStr = amt.GetProperty("value").GetString() ?? "0";
                currency = amt.GetProperty("currency_code").GetString() ?? "";
                capturedAmount = decimal.Parse(valStr, CultureInfo.InvariantCulture);
                capturedAmount = decimal.Round(capturedAmount, 2, MidpointRounding.AwayFromZero);
            }
        }

        // verify captured amount vs DB net amount (tolerance 0.01)
        decimal expected = decimal.Round(payment.NetAmount, 2, MidpointRounding.AwayFromZero);
        if (Math.Abs(capturedAmount - expected) > 0.01m)
        {
            payment.Status = "AMOUNT_MISMATCH";
            await _db.SaveChangesAsync();
            _logger.LogWarning("Captured amount mismatch. PaymentId:{PaymentId} Expected:{Expected} Captured:{Captured} OrderId:{OrderId}", payment.Id, expected, capturedAmount, request.OrderId);
            throw new InvalidOperationException($"Captured amount {capturedAmount} {currency} does not match expected {expected}");
        }

        // success
        payment.Status = "COMPLETED";
        await _db.SaveChangesAsync();

        return new CapturePaymentResponseDto
        {
            Status = "COMPLETED",
            CapturedAmount = capturedAmount,
            Currency = currency,
            RawResponse = JsonSerializer.Deserialize<object>(json)
        };
    }
}
