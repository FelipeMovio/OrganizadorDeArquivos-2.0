
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using OrganizadorDePasta_2._0.Models;
using OrganizadorDePasta_2._0.Repository;
using OrganizadorDePasta_2._0.Service;
using OrganizadorDePasta_2._0.ViewModels;
using System.Windows;

namespace OrganizadorDePasta_2._0;

public partial class MainWindow : Window
{
    private readonly IDbContextFactory<OrganizadorDbContext> _dbContextFactory;
    private readonly ILoggerFactory _loggerFactory;
    private readonly MainViewModel _viewModel;
    private readonly OrganizadorService _organizador;
    private readonly HistoricoService _historicoService;

    private MonitoramentoService? _monitoramento;

    public MainWindow(IDbContextFactory<OrganizadorDbContext> dbContextFactory)
    { 
        InitializeComponent();

        _dbContextFactory = dbContextFactory;
        // Configura os logs para aparecerem na janela Saída do Visual Studio.
        _loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .SetMinimumLevel(LogLevel.Debug)
                .AddDebug();
        });

        _viewModel = new MainViewModel();
        DataContext = _viewModel;

        // Carrega as configurações da aplicação.
        ConfiguracaoService configuracaoService =
            new ConfiguracaoService();

        Configuracao configuracao =
            configuracaoService.CarregarConfiguracao();

        // Cria o serviço de histórico.
        _historicoService = new HistoricoService();

        if (_historicoService.AvisoCarregamento != null)
        {
            MessageBox.Show(
                _historicoService.AvisoCarregamento,
                "Aviso sobre o histórico",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        // Cria um logger específico para o OrganizadorService.
        ILogger<OrganizadorService> loggerOrganizador =
            _loggerFactory.CreateLogger<OrganizadorService>();

        _organizador = new OrganizadorService(
            configuracao,
            _historicoService,
            loggerOrganizador);
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
            // Organiza os arquivos que já estão na pasta.
            int quantidadeInicial =
                _organizador.OrganizarPasta(caminhoPasta);

            _viewModel.ArquivosOrganizados =
                quantidadeInicial;

            // Cria o logger específico do monitoramento.
            ILogger<MonitoramentoService> loggerMonitoramento =
                _loggerFactory.CreateLogger<MonitoramentoService>();

            _monitoramento = new MonitoramentoService(
                _organizador,
                caminhoPasta,
                loggerMonitoramento);

            _monitoramento.ArquivoOrganizado += AtualizarContador;
            _monitoramento.Mensagem += ExibirMensagem;

            _viewModel.Status =
                "🟢 Monitoramento ativo";

            _viewModel.Mensagem =
                "Monitoramento iniciado com sucesso.";
        }
        catch (Exception ex)
        {
            _monitoramento?.Dispose();
            _monitoramento = null;

            _viewModel.Status =
                "🔴 Monitoramento parado";

            _viewModel.Mensagem =
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

        _viewModel.Mensagem =
            "Monitoramento encerrado.";
    }

    protected override void OnClosed(EventArgs e)
    {
        // Encerra o monitoramento antes de liberar os loggers.
        _monitoramento?.Dispose();
        _monitoramento = null;

        _loggerFactory.Dispose();

        base.OnClosed(e);
    }

    private void AtualizarContador()
    {
        Dispatcher.Invoke(() =>
        {
            _viewModel.ArquivosOrganizados++;
        });
    }

    private void ZerarContador()
    {
        _viewModel.ArquivosOrganizados = 0;
    }

    private void ExibirMensagem(string mensagem)
    {
        Dispatcher.Invoke(() =>
        {
            _viewModel.Mensagem = mensagem;
        });
    }
}
