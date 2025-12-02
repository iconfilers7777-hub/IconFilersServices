using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IconFilers.Infrastructure.Persistence.Entities
{
    [Keyless]
    public class Roles
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class DocTypes
    {
        public string type {  get; set; }
    }

    public class DocCount
    {
        public string Status { get; set; }
        public string Count { get; set; }
    }
}
