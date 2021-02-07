using Microsoft.EntityFrameworkCore;
using APIAcoes.Models;

namespace APIAcoes.Data
{
    public class AcoesContext : DbContext
    {
        public DbSet<HistoricoAcao> HistoricoAcoes { get; set; }

        public AcoesContext(DbContextOptions<AcoesContext> options) :
            base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<HistoricoAcao>()
                .HasKey(c => c.Id);
        }
    }
}