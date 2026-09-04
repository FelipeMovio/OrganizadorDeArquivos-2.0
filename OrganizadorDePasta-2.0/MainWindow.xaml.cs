using System.IO;
using System.Windows;
using OrganizadorDePasta_2._0.Models;
using OrganizadorDePasta_2._0.Service;
using Microsoft.Win32;

namespace OrganizadorDePasta_2._0;

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

        var pastaDownloads = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.UserProfile),
            "Downloads");


        // Cria o serviço de monitoramento.
        _monitoramento =
            new MonitoramentoService(_organizador, pastaDownloads);
    }

    private void SelecionarPasta_Click(
        object sender,
        RoutedEventArgs e)
    {
        // Cria a janela para seleção de uma pasta.
        var dialog = new OpenFolderDialog();

        // Abre a janela.
        if (dialog.ShowDialog() == true)
        {
            // Exibe o caminho escolhido pelo usuário
            // no TextBox da interface.
            PastaMonitoradaTextBox.Text =
                dialog.FolderName;
        }
    }



    // ORGANIZAÇÃO MANUAL
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


    // ENCERRAMENTO DA APLICAÇÃO
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