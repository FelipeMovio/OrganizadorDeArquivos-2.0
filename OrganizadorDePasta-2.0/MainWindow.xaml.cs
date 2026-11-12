
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using OrganizadorDePasta_2._0.Models;
using OrganizadorDePasta_2._0.Repository;
using OrganizadorDePasta_2._0.Service;
using OrganizadorDePasta_2._0.ViewModels;
using System.Threading.Tasks;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace OrganizadorDePasta_2._0;

public partial class MainWindow : Window
{
    private bool _iniciandoMonitoramento;
    private System.Windows.Forms.NotifyIcon? _notifyIcon;
    private System.Windows.Forms.ToolStripMenuItem? _pausarRetomarItem;

    private bool _encerrandoAplicacao;

    private readonly IDbContextFactory<OrganizadorDbContext> _dbContextFactory;
    private readonly ILoggerFactory _loggerFactory;
    private readonly MainViewModel _viewModel;
    private readonly OrganizadorService _organizador;
    private readonly HistoricoService _historicoService;

    private MonitoramentoService? _monitoramento;

    public MainWindow(
        IDbContextFactory<OrganizadorDbContext> dbContextFactory)
    {
        InitializeComponent();
        ConfigurarSystemTray();

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
        _historicoService = new HistoricoService(_dbContextFactory);

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
            PastaMonitoradaTextBox.Text = dialog.FolderName;
        }
    }

    private async void IniciarMonitoramento_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(PastaMonitoradaTextBox.Text))
        {
            _viewModel.Mensagem =
                "Selecione uma pasta antes de iniciar o monitoramento.";

            return;
        }

        if (_monitoramento != null || _iniciandoMonitoramento)
        {
            return;
        }

        _iniciandoMonitoramento = true;

        string caminhoPasta = PastaMonitoradaTextBox.Text;

        _viewModel.Status = "🟡 Preparando monitoramento...";
        _viewModel.Mensagem = "Organizando arquivos existentes...";

        try
        {
            // Executa a organização fora da UI thread.
            int quantidadeInicial = await Task.Run(
                () => _organizador.OrganizarPasta(caminhoPasta));

            _viewModel.ArquivosOrganizados = quantidadeInicial;

            ILogger<MonitoramentoService> loggerMonitoramento =
                _loggerFactory.CreateLogger<MonitoramentoService>();

            _monitoramento = new MonitoramentoService(
                _organizador,
                caminhoPasta,
                loggerMonitoramento);

            _monitoramento.ArquivoOrganizado += AtualizarContador;
            _monitoramento.Mensagem += ExibirMensagem;

            AtualizarMenuPausa(true);

            _viewModel.Status = "🟢 Monitoramento ativo";
            _viewModel.Mensagem = "Monitoramento iniciado com sucesso.";
        }
        catch (Exception ex)
        {
            _monitoramento?.Dispose();
            _monitoramento = null;

            AtualizarMenuPausa(false);

            _viewModel.Status = "🔴 Monitoramento parado";
            _viewModel.Mensagem =
                $"Erro ao iniciar o monitoramento: {ex.Message}";
        }
        finally
        {
            _iniciandoMonitoramento = false;
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

        AtualizarMenuPausa(false);

        ZerarContador();

        _viewModel.Status = "🔴 Monitoramento parado";
        _viewModel.Mensagem = "Monitoramento encerrado.";
    }

    private void AlternarPausaMonitoramento()
    {
        Dispatcher.Invoke(() =>
        {
            if (_monitoramento == null)
            {
                return;
            }

            if (_monitoramento.EstaPausado)
            {
                _monitoramento.Retomar();

                _viewModel.Status = "🟢 Monitoramento ativo";
                _viewModel.Mensagem = "Monitoramento retomado.";
            }
            else
            {
                _monitoramento.Pausar();

                _viewModel.Status = "🟡 Monitoramento pausado";
                _viewModel.Mensagem = "Monitoramento pausado.";
            }

            AtualizarMenuPausa(true);
        });
    }

    private void AtualizarMenuPausa(bool habilitado)
    {
        if (_pausarRetomarItem == null)
        {
            return;
        }

        _pausarRetomarItem.Enabled = habilitado;

        if (!habilitado || _monitoramento == null)
        {
            _pausarRetomarItem.Text = "Pausar monitoramento";
            return;
        }

        _pausarRetomarItem.Text = _monitoramento.EstaPausado
            ? "Retomar monitoramento"
            : "Pausar monitoramento";
    }

    private void Window_StateChanged(
        object? sender,
        EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
        {
            Hide();
            ShowInTaskbar = false;
        }
    }

    protected override void OnClosing(
        System.ComponentModel.CancelEventArgs e)
    {
        if (!_encerrandoAplicacao)
        {
            e.Cancel = true;

            Hide();
            ShowInTaskbar = false;
        }

        base.OnClosing(e);
    }

    protected override void OnClosed(EventArgs e)
    {
        _monitoramento?.Dispose();
        _monitoramento = null;

        if (_notifyIcon != null)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _notifyIcon = null;
        }

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

    private void ConfigurarSystemTray()
    {
        var menu = new System.Windows.Forms.ContextMenuStrip();

        var abrirItem =
            new System.Windows.Forms.ToolStripMenuItem("Abrir");

        abrirItem.Click += (_, _) => AbrirJanela();

        _pausarRetomarItem =
            new System.Windows.Forms.ToolStripMenuItem("Pausar monitoramento")
            {
                Enabled = false
            };

        _pausarRetomarItem.Click += (_, _) =>
            AlternarPausaMonitoramento();

        var configuracoesItem =
            new System.Windows.Forms.ToolStripMenuItem("Configurações");

        configuracoesItem.Click += (_, _) => AbrirJanela();

        var sairItem =
            new System.Windows.Forms.ToolStripMenuItem("Sair");

        sairItem.Click += (_, _) =>
        {
            _encerrandoAplicacao = true;
            Close();
        };

        menu.Items.Add(abrirItem);
        menu.Items.Add(_pausarRetomarItem);
        menu.Items.Add(configuracoesItem);
        menu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
        menu.Items.Add(sairItem);

        _notifyIcon = new System.Windows.Forms.NotifyIcon
        {
            Icon = System.Drawing.SystemIcons.Application,
            Text = "Organizador de Pasta",
            ContextMenuStrip = menu,
            Visible = true
        };

        _notifyIcon.DoubleClick += (_, _) => AbrirJanela();
    }

    private void AbrirJanela()
    {
        Show();

        ShowInTaskbar = true;
        WindowState = WindowState.Normal;

        Activate();
    }
}
