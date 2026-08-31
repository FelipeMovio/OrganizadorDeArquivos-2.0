using System.IO;
using System.Windows;
using OrganizadorDePasta_2._0.Models;
using OrganizadorDePasta_2._0.Service;

namespace OrganizadorDePasta_2._0;

// Atualmente o WPF está sendo utilizado como ponto de entrada
// temporário para testar o núcleo da aplicação.
//
// Em uma fase futura a interface poderá ser estruturada
// utilizando MVVM.
public partial class MainWindow : Window
{
    // Serviço responsável pela organização dos arquivos.
    private readonly OrganizadorService _organizador;

    // Serviço responsável pelo monitoramento da pasta.
    private readonly MonitoramentoService _monitoramento;


    public MainWindow()
    {
        InitializeComponent();


        // Cria o serviço responsável por carregar
        // as configurações da aplicação.
        ConfiguracaoService configuracaoService =
            new ConfiguracaoService();


        // Carrega as regras do arquivo de configuração.
        Configuracao configuracao =
            configuracaoService.CarregarConfiguracao();


        // Cria o serviço responsável por organizar
        // os arquivos.
        _organizador =
            new OrganizadorService(configuracao);


        // Cria o serviço de monitoramento.
        //
        // A partir deste momento o FileSystemWatcher
        // começa a observar a pasta Downloads.
        _monitoramento =
            new MonitoramentoService(_organizador);
    }


    // ============================================================
    // ORGANIZAÇÃO MANUAL
    // ============================================================

    private void OrganizarDownloads_Click(
        object sender,
        RoutedEventArgs e)
    {
        // Obtém o caminho da pasta Downloads
        // do usuário atual.
        var pastaTeste = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.UserProfile),
            "Downloads");


        // Organiza os arquivos que já estavam
        // na pasta.
        //
        // Isso continua existindo porque o monitoramento
        // trabalha principalmente com arquivos novos.
        _organizador.OrganizarPasta(pastaTeste);
    }


    // ============================================================
    // ENCERRAMENTO DA APLICAÇÃO
    // ============================================================

    protected override void OnClosed(EventArgs e)
    {
        // Quando a janela for fechada,
        // encerramos o FileSystemWatcher.
        //
        // Isso evita deixar recursos abertos.
        _monitoramento.Dispose();


        // Continua o processo normal de fechamento
        // da janela WPF.
        base.OnClosed(e);
    }
}