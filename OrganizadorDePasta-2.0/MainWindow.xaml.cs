using System.IO;
using System.Windows;
using OrganizadorDePasta_2._0.Models;
using OrganizadorDePasta_2._0.Service;

namespace OrganizadorDePasta_2._0;

// Atualmente o WPF está sendo utilizado apenas como ponto de entrada
// temporário para testar o núcleo da aplicação.
// A interface será estruturada corretamente com MVVM em uma etapa futura.
public partial class MainWindow : Window
{

    private readonly OrganizadorService _organizador;
    private readonly MonitoramentoService _monitoramento;
    public MainWindow()
    {
        InitializeComponent();
        InitializeComponent();

        var pastaTeste = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Downloads");

        ConfiguracaoService configuracaoService =
            new ConfiguracaoService();

        Configuracao configuracao =
            configuracaoService.CarregarConfiguracao();

        _organizador =
            new OrganizadorService(configuracao);

        _monitoramento =
            new MonitoramentoService(_organizador);
    }

    private void OrganizarDownloads_Click(object sender, RoutedEventArgs e)
    {
        var pastaTeste = Path.Combine(
           Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
           "Downloads");

        _organizador.OrganizarPasta(pastaTeste);

    }
}