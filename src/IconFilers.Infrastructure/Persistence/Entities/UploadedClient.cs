using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IconFilers.Infrastructure.Persistence.Entities
{
    [Keyless]
    public class UploadedClient
    {      
      public string Name { get; set; }
      public string Contact { get; set; }
      public string Email { get; set; }
      public string Status { get; set; }
      public string Contact2 { get; set; }

    }
}
