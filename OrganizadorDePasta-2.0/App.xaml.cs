
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using OrganizadorDePasta_2._0.Repository;
using System.Windows;

namespace OrganizadorDePasta_2._0;

public partial class App : Application
{
    public IDbContextFactory<OrganizadorDbContext> DbContextFactory
    { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        string connectionString =
            configuration.GetConnectionString("OrganizadorConnection")
            ?? throw new InvalidOperationException(
                "A connection string OrganizadorConnection não foi encontrada.");

        DbContextFactory =
            new PooledDbContextFactory<OrganizadorDbContext>(
                new DbContextOptionsBuilder<OrganizadorDbContext>()
                    .UseSqlServer(connectionString)
                    .Options);

        var mainWindow = new MainWindow(DbContextFactory);
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (DbContextFactory is IDisposable disposable)
            disposable.Dispose();

        base.OnExit(e);
    }
}
