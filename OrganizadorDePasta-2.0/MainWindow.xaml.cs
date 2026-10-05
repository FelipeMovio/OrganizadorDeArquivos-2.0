using Microsoft.Win32;
using OrganizadorDePasta_2._0.Models;
using OrganizadorDePasta_2._0.Service;
using OrganizadorDePasta_2._0.ViewModels;
using System.Windows;

namespace OrganizadorDePasta_2._0;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly OrganizadorService _organizador;

    private MonitoramentoService? _monitoramento;

    private int _arquivosOrganizados;

    public MainWindow()
    {

        InitializeComponent();

        _viewModel = new MainViewModel();

        DataContext = _viewModel;

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
            MensagemTextBlock.Text =
                "Selecione uma pasta antes de iniciar o monitoramento.";

            return;
        }

        if (_monitoramento != null)
        {
            return;
        }

        string caminhoPasta =
            PastaMonitoradaTextBox.Text;

        try
        {
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

            _viewModel.Status =
                "🟢 Monitoramento ativo";

            MensagemTextBlock.Text =
                "Monitoramento iniciado com sucesso.";
        }
        catch (Exception ex)
        {
            _monitoramento?.Dispose();

            _monitoramento = null;

            _viewModel.Status =
                "🔴 Monitoramento parado";

            MensagemTextBlock.Text =
                $"Erro ao iniciar o monitoramento: {ex.Message}";
        }
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

        _viewModel.Status =
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