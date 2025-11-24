using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace IconFilers.Application.DTOs
{
    public class CapturePaymentRequestDto
    {
        public int PaymentId { get; set; }       // internal DB id
        [Required, MinLength(1)]
        public string OrderId { get; set; } = ""; // PayPal order id
    }
}
