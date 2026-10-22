using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OrganizadorDePasta_2._0.Repository;
using System.Windows;

namespace OrganizadorDePasta_2._0;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public OrganizadorDbContext DbContext { get; private set; } = null!;

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

        var options = new DbContextOptionsBuilder<OrganizadorDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        DbContext = new OrganizadorDbContext(options);

        var mainWindow = new MainWindow(DbContext);
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        DbContext?.Dispose();
        base.OnExit(e);
    }
}
