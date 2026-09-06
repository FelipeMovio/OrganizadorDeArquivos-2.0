using System.IO;
using System.Windows;
using Microsoft.Win32;
using OrganizadorDePasta_2._0.Models;
using OrganizadorDePasta_2._0.Service;

namespace OrganizadorDePasta_2._0;

public partial class MainWindow : Window
{
    private readonly OrganizadorService _organizador;

    private MonitoramentoService? _monitoramento;

    public MainWindow()
    {
        InitializeComponent();

        // Carrega as configurações e cria o serviço de organização.
        ConfiguracaoService configuracaoService =
            new ConfiguracaoService();

        Configuracao configuracao =
            configuracaoService.CarregarConfiguracao();

        _organizador =
            new OrganizadorService(configuracao);
    }

    private void SelecionarPasta_Click(
        object sender,
        RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog();

        if (dialog.ShowDialog() == true)
        {
            PastaMonitoradaTextBox.Text =
                dialog.FolderName;
        }
    }

    private void IniciarMonitoramento_Click(
        object sender,
        RoutedEventArgs e)
    {
        // Verifica se uma pasta foi selecionada.
        if (string.IsNullOrWhiteSpace(
            PastaMonitoradaTextBox.Text))
        {
            MessageBox.Show(
                "Selecione uma pasta antes de iniciar o monitoramento.");

            return;
        }

        string caminhoPasta =
            PastaMonitoradaTextBox.Text;

        // Organiza os arquivos que já existem na pasta.
        _organizador.OrganizarPasta(caminhoPasta);

        // Inicia o monitoramento da pasta selecionada.
        _monitoramento =
            new MonitoramentoService(
                _organizador,
                caminhoPasta);

        StatusTextBlock.Text =
            "🟢 Monitoramento ativo";
    }

    private void PararMonitoramento_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (_monitoramento == null)
        {
            return;
        }

        _monitoramento.Dispose();

        

        StatusTextBlock.Text =
            "🔴 Monitoramento parado";
    }

    protected override void OnClosed(EventArgs e)
    {
        // Garante que o monitoramento seja encerrado ao fechar a janela.
        _monitoramento?.Dispose();

        base.OnClosed(e);
    }
}