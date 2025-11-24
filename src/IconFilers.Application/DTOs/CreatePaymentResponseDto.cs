using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IconFilers.Application.DTOs;

public class CreatePaymentResponseDto
{
    public int PaymentId { get; set; }
    public string OrderId { get; set; } = "";
    public string ApproveUrl { get; set; } = "";
}
