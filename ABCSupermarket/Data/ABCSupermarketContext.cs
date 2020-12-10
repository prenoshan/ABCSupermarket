using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ABCSupermarket.Models
{
    public class ABCSupermarketContext : DbContext
    {
        public ABCSupermarketContext (DbContextOptions<ABCSupermarketContext> options)
            : base(options)
        {
        }

        public DbSet<ABCSupermarket.Models.Product> Product { get; set; }
    }
}
