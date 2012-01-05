using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Objects;

namespace EFCodeFirstCache.Test
{
    public class ProductContext : DbContext, IObjectContextAdapter
    {
        public ProductContext()
            : base("ProductConnection")
        {

        }

        public DbSet<Product> Products { get; set; }

        public ObjectContext UnderlyingContext
        { get { return ((IObjectContextAdapter)this).ObjectContext; } }
    }
}
