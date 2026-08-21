using System.IO;
using System.Windows;
using OrganizadorDePasta_2._0.Service;

namespace OrganizadorDePasta_2._0;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OrganizarDownloads_Click(object sender, RoutedEventArgs e)
    {
        var pastaTeste = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Downloads");

        var organizador = new OrganizadorService();

        organizador.OrganizarPasta(pastaTeste);
    }
}