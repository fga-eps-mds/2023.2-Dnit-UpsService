using Entidades;
using Microsoft.EntityFrameworkCore;

namespace app.Entidades
{
    public class AppDbContext : DbContext
    {
        public DbSet<Sinistro> Sinistros { get; set; }
        public DbSet<Rodovia> Rodovias { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .HasDbFunction(typeof(AppDbContext)
                    .GetMethod(nameof(CalcularDistancia),
                        new[] { typeof(double), typeof(double), typeof(double), typeof(double) })!)
                .HasName("CalcularDistancia");
        }

        /// <summary>
        /// O EF Core traduz essa a chamada dessa função em uma stored procedure.
        /// Mais detalhes: https://learn.microsoft.com/en-us/ef/core/querying/user-defined-function-mapping
        /// </summary>
        /// <param name="lat1"></param>
        /// <param name="long1"></param>
        /// <param name="lat2"></param>
        /// <param name="long2"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public double CalcularDistancia(double lat1, double long1, double lat2, double long2)
            => throw new NotSupportedException("Essa função não deve ser chamada no cliente");
    }
}