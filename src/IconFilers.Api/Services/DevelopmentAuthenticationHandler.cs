using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace IconFilers.Api.Services
{
    // Development-only authentication handler that auto-authenticates requests.
    // Do NOT use in production.
    public class DevelopmentAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public DevelopmentAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Create a principal that has Admin and User roles for local development
            // Use a stable GUID for development so GetMyAssignments can match AssignedTo values in DB
            var devGuid = "76864361-6D00-459C-BF5A-0480DB37FF09";
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, devGuid),
                new Claim(ClaimTypes.Name, "Development User"),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim(ClaimTypes.Role, "User")
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
