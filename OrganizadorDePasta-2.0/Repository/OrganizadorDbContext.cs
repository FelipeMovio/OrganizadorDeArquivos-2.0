using Microsoft.EntityFrameworkCore;
using OrganizadorDePasta_2._0.Models;

namespace OrganizadorDePasta_2._0.Repository;

public class OrganizadorDbContext : DbContext
{
    public OrganizadorDbContext(DbContextOptions<OrganizadorDbContext> opts)
    : base(opts)
    {

    }

    public DbSet<HistoricoArquivo> historicoArquivos { get; set; }
}
