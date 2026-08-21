using System.IO;
using System.Windows;
using OrganizadorDePasta_2._0.Service;

namespace OrganizadorDePasta_2._0;

// Atualmente o WPF está sendo utilizado apenas como ponto de entrada
// temporário para testar o núcleo da aplicação.
// A interface será estruturada corretamente com MVVM em uma etapa futura.
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OrganizarDownloads_Click(object sender, RoutedEventArgs e)
    {
        // Pasta utilizada durante o desenvolvimento para evitar
        // alterações acidentais na pasta Downloads real do usuário.
        var pastaTeste = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Downloads");

        // Cria uma instância do serviço responsável pela organização.
        var organizador = new OrganizadorService();

        // Inicia o processo de organização da pasta informada.
        organizador.OrganizarPasta(pastaTeste);
    }
}