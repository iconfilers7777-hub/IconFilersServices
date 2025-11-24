using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IconFilers.Application.DTOs
{
    public class PayPalOptions
    {
        public string ClientId { get; set; } = "";
        public string Secret { get; set; } = "";
        public bool UseSandbox { get; set; } = true;
    }
}
