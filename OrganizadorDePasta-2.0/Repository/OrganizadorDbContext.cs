
using Microsoft.EntityFrameworkCore;
using OrganizadorDePasta_2._0.Models;

namespace OrganizadorDePasta_2._0.Repository;

public class OrganizadorDbContext : DbContext
{
    public OrganizadorDbContext(
        DbContextOptions<OrganizadorDbContext> options)
        : base(options)
    {
    }

    public DbSet<HistoricoArquivo> HistoricoArquivos
        => Set<HistoricoArquivo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<HistoricoArquivo>(entity =>
        {
            entity.ToTable("HistoricoArquivos");

            entity.HasKey(h => h.Id);

            entity.Property(h => h.NomeArquivo)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(h => h.CaminhoOrigem)
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(h => h.CaminhoDestino)
                .HasMaxLength(1000);

            entity.Property(h => h.DataHora)
                .HasColumnType("datetime2")
                .IsRequired();

            entity.Property(h => h.Sucesso)
                .IsRequired();

            entity.Property(h => h.Mensagem)
                .IsRequired();
        });
    }
}
