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

    private int _arquivosOrganizados;

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
        if (string.IsNullOrWhiteSpace(
            PastaMonitoradaTextBox.Text))
        {
            MessageBox.Show(
                "Selecione uma pasta antes de iniciar o monitoramento.");

            return;
        }

        if (_monitoramento != null)
        {
            return;
        }

        string caminhoPasta =
            PastaMonitoradaTextBox.Text;

        int quantidadeInicial =
            _organizador.OrganizarPasta(caminhoPasta);

        _arquivosOrganizados = quantidadeInicial;

        ArquivosOrganizadosTextBlock.Text =
            $"Arquivos organizados: {_arquivosOrganizados}";


        _monitoramento =
            new MonitoramentoService(
                _organizador,
                caminhoPasta);

        _monitoramento.ArquivoOrganizado += AtualizarContador;
        _monitoramento.Mensagem += ExibirMensagem;

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

        _monitoramento = null;

        ZerarContador();

        StatusTextBlock.Text =
            "🔴 Monitoramento parado";
    }

    protected override void OnClosed(EventArgs e)
    {
        // Garante que o monitoramento seja encerrado ao fechar a janela.
        _monitoramento?.Dispose();

        base.OnClosed(e);
    }

    private void AtualizarContador()
    {
        Dispatcher.Invoke(() =>
        {
            _arquivosOrganizados++;

            ArquivosOrganizadosTextBlock.Text =
                $"Arquivos organizados: {_arquivosOrganizados}";
        });
    }

    private void ZerarContador()
    {
        _arquivosOrganizados = 0;

        ArquivosOrganizadosTextBlock.Text =
            $"Arquivos organizados: {_arquivosOrganizados}";
    }

    private void ExibirMensagem(string mensagem)
    {
        Dispatcher.Invoke(() =>
        {
            MensagemTextBlock.Text = mensagem;
        });
    }
}