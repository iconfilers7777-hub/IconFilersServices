using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace IconFilers.Infrastructure.Persistence.Entities;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
   
}
