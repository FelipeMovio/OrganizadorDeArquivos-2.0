
using Microsoft.Extensions.Logging;
using System.IO;

namespace OrganizadorDePasta_2._0.Service;

public class MonitoramentoService : IDisposable
{
    private readonly FileSystemWatcher _watcher;
    private readonly OrganizadorService _organizadorService;
    private readonly ILogger<MonitoramentoService> _logger;

    private readonly HashSet<string> _arquivosProcessando = new();
    private readonly object _lock = new();

    private const int MaxTentativas = 10;
    private const int TempoEspera = 1000;

    private readonly string _caminhoPasta;

    private bool _disposed;

    public event Action? ArquivoOrganizado;
    public event Action<string>? Mensagem;

    public MonitoramentoService(
        OrganizadorService organizadorService,
        string caminhoPasta,
        ILogger<MonitoramentoService> logger)
    {
        _organizadorService = organizadorService;
        _caminhoPasta = caminhoPasta;
        _logger = logger;

        if (!Directory.Exists(_caminhoPasta))
        {
            _logger.LogError(
                "A pasta de monitoramento não existe: {CaminhoPasta}",
                _caminhoPasta);

            throw new DirectoryNotFoundException(
                $"A pasta monitorada não existe: {_caminhoPasta}");
        }

        _watcher = new FileSystemWatcher
        {
            Path = _caminhoPasta,
            NotifyFilter =
                NotifyFilters.FileName |
                NotifyFilters.LastWrite |
                NotifyFilters.Size
        };

        _watcher.Created += ArquivoCriado;
        _watcher.Renamed += ArquivoRenomeado;
        _watcher.Error += WatcherErro;

        _watcher.EnableRaisingEvents = true;

        _logger.LogInformation(
            "Monitoramento iniciado na pasta {CaminhoPasta}.",
            _caminhoPasta);
    }

    private void ArquivoCriado(
        object sender,
        FileSystemEventArgs e)
    {
        _logger.LogDebug(
            "Evento de criação detectado para {CaminhoArquivo}.",
            e.FullPath);

        if (DeveIgnorarArquivo(e.FullPath))
        {
            return;
        }

        ProcessarArquivo(e.FullPath);
    }

    private void ArquivoRenomeado(
        object sender,
        RenamedEventArgs e)
    {
        _logger.LogDebug(
            "Arquivo renomeado de {Origem} para {Destino}.",
            e.OldFullPath,
            e.FullPath);

        bool eraDownloadTemporario =
            e.OldFullPath.EndsWith(
                ".crdownload",
                StringComparison.OrdinalIgnoreCase)
            ||
            e.OldFullPath.EndsWith(
                ".part",
                StringComparison.OrdinalIgnoreCase);

        if (!eraDownloadTemporario)
        {
            return;
        }

        _logger.LogInformation(
            "Possível download concluído: {CaminhoArquivo}.",
            e.FullPath);

        if (DeveIgnorarArquivo(e.FullPath))
        {
            return;
        }

        ProcessarArquivo(e.FullPath);
    }

    private void ProcessarArquivo(string caminhoArquivo)
    {
        if (Directory.Exists(caminhoArquivo))
        {
            _logger.LogDebug(
                "Diretório ignorado: {CaminhoArquivo}.",
                caminhoArquivo);

            return;
        }

        lock (_lock)
        {
            if (_disposed)
            {
                return;
            }

            if (!_arquivosProcessando.Add(caminhoArquivo))
            {
                _logger.LogDebug(
                    "Arquivo já está sendo processado: {CaminhoArquivo}.",
                    caminhoArquivo);

                return;
            }
        }

        _ = Task.Run(async () =>
        {
            try
            {
                // Dá tempo para o arquivo terminar de ser gravado.
                await Task.Delay(TempoEspera);

                OrganizarComRetry(caminhoArquivo);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro inesperado ao processar {CaminhoArquivo}.",
                    caminhoArquivo);

                Notificar(
                    $"Ocorreu um erro inesperado ao processar " +
                    $"'{Path.GetFileName(caminhoArquivo)}'.");
            }
            finally
            {
                lock (_lock)
                {
                    _arquivosProcessando.Remove(caminhoArquivo);
                }
            }
        });
    }

    private void OrganizarComRetry(string caminhoArquivo)
    {
        for (int tentativa = 1;
             tentativa <= MaxTentativas;
             tentativa++)
        {
            try
            {
                if (!File.Exists(caminhoArquivo))
                {
                    _logger.LogWarning(
                        "O arquivo não existe mais e será ignorado: {CaminhoArquivo}.",
                        caminhoArquivo);

                    Notificar(
                        $"O arquivo '{Path.GetFileName(caminhoArquivo)}' " +
                        "não existe mais.");

                    return;
                }

                _logger.LogDebug(
                    "Tentativa {Tentativa}/{MaxTentativas} para organizar {CaminhoArquivo}.",
                    tentativa,
                    MaxTentativas,
                    caminhoArquivo);

                _organizadorService.OrganizarArquivo(caminhoArquivo);

                _logger.LogInformation(
                    "Arquivo organizado com sucesso: {CaminhoArquivo}.",
                    caminhoArquivo);

                ArquivoOrganizado?.Invoke();

                Notificar(
                    $"Arquivo '{Path.GetFileName(caminhoArquivo)}' " +
                    "organizado com sucesso.");

                return;
            }
            catch (IOException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Erro de entrada/saída na tentativa {Tentativa}/{MaxTentativas} " +
                    "para {CaminhoArquivo}.",
                    tentativa,
                    MaxTentativas,
                    caminhoArquivo);

                if (tentativa == MaxTentativas)
                {
                    RegistrarFalhaFinal(
                        caminhoArquivo,
                        "Não foi possível organizar o arquivo após várias tentativas.");

                    return;
                }

                Notificar(
                    $"Aguardando o arquivo " +
                    $"'{Path.GetFileName(caminhoArquivo)}' ficar disponível...");

                Thread.Sleep(TempoEspera);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Acesso negado na tentativa {Tentativa}/{MaxTentativas} " +
                    "para {CaminhoArquivo}.",
                    tentativa,
                    MaxTentativas,
                    caminhoArquivo);

                if (tentativa == MaxTentativas)
                {
                    RegistrarFalhaFinal(
                        caminhoArquivo,
                        "Acesso negado ao arquivo após várias tentativas.");

                    return;
                }

                Notificar(
                    $"Acesso negado ao arquivo " +
                    $"'{Path.GetFileName(caminhoArquivo)}'. " +
                    "Uma nova tentativa será realizada.");

                Thread.Sleep(TempoEspera);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro inesperado ao organizar {CaminhoArquivo}.",
                    caminhoArquivo);

                RegistrarFalhaFinal(
                    caminhoArquivo,
                    $"Erro inesperado: {ex.Message}");

                return;
            }
        }
    }

    private void RegistrarFalhaFinal(
        string caminhoArquivo,
        string mensagem)
    {
        _logger.LogError(
            "Não foi possível organizar {CaminhoArquivo}. Motivo: {Mensagem}",
            caminhoArquivo,
            mensagem);

        try
        {
            _organizadorService.RegistrarFalha(
                caminhoArquivo,
                mensagem);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Não foi possível registrar a falha no histórico para {CaminhoArquivo}.",
                caminhoArquivo);
        }

        Notificar(
            $"Falha ao organizar '{Path.GetFileName(caminhoArquivo)}'.");
    }

    private bool DeveIgnorarArquivo(string caminhoArquivo)
    {
        if (Directory.Exists(caminhoArquivo))
        {
            _logger.LogDebug(
                "Diretório ignorado: {CaminhoArquivo}.",
                caminhoArquivo);

            return true;
        }

        string nomeArquivo = Path.GetFileName(caminhoArquivo);

        if (nomeArquivo.EndsWith(
                ".crdownload",
                StringComparison.OrdinalIgnoreCase)
            ||
            nomeArquivo.EndsWith(
                ".part",
                StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogDebug(
                "Arquivo temporário ignorado: {NomeArquivo}.",
                nomeArquivo);

            return true;
        }

        return false;
    }

    private void WatcherErro(
        object sender,
        ErrorEventArgs e)
    {
        Exception erro = e.GetException();

        _logger.LogError(
            erro,
            "O FileSystemWatcher encontrou um erro na pasta {CaminhoPasta}.",
            _caminhoPasta);

        Notificar(
            "Ocorreu um erro durante o monitoramento da pasta.");
    }

    private void Notificar(string mensagem)
    {
        try
        {
            Mensagem?.Invoke(mensagem);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Um assinante do evento Mensagem lançou uma exceção.");
        }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
        }

        _watcher.EnableRaisingEvents = false;

        _watcher.Created -= ArquivoCriado;
        _watcher.Renamed -= ArquivoRenomeado;
        _watcher.Error -= WatcherErro;

        _watcher.Dispose();

        lock (_lock)
        {
            _arquivosProcessando.Clear();
        }

        _logger.LogInformation(
            "Monitoramento encerrado na pasta {CaminhoPasta}.",
            _caminhoPasta);

        Notificar("Monitoramento encerrado.");
    }
}
