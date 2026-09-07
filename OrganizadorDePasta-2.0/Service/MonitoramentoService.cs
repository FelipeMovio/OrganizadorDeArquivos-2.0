using System.Diagnostics;
using System.IO;

namespace OrganizadorDePasta_2._0.Service;

public class MonitoramentoService : IDisposable
{
    private readonly FileSystemWatcher _watcher;
    private readonly OrganizadorService _organizadorService;

    private readonly HashSet<string> _arquivosProcessando = new();
    private readonly object _lock = new();

    private const int MaxTentativas = 10;
    private const int TempoEspera = 1000;

    private readonly string _caminhoPasta;

    public MonitoramentoService(
        OrganizadorService organizadorService,
        string caminhoPasta)
    {
        _organizadorService = organizadorService;
        _caminhoPasta = caminhoPasta;

        if (!Directory.Exists(_caminhoPasta))
        {
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

        Debug.WriteLine(
            $"[MONITORAMENTO] Iniciado: {_caminhoPasta}");
    }

    private void ArquivoCriado(
        object sender,
        FileSystemEventArgs e)
    {
        Debug.WriteLine(
            $"[CREATED] {e.FullPath}");

        // Ignora arquivos temporários e diretórios.
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
        Debug.WriteLine(
            $"[RENAMED] {e.OldFullPath} -> {e.FullPath}");

        // Detecta quando um download temporário é finalizado.
        if (e.OldFullPath.EndsWith(
                ".crdownload",
                StringComparison.OrdinalIgnoreCase))
        {
            Debug.WriteLine(
                $"[DOWNLOAD] Download concluído: {e.FullPath}");

            ProcessarArquivo(e.FullPath);

            return;
        }

        if (e.OldFullPath.EndsWith(
                ".part",
                StringComparison.OrdinalIgnoreCase))
        {
            Debug.WriteLine(
                $"[DOWNLOAD] Download concluído: {e.FullPath}");

            ProcessarArquivo(e.FullPath);

            return;
        }
    }

    private void ProcessarArquivo(
        string caminhoArquivo)
    {
        if (Directory.Exists(caminhoArquivo))
        {
            return;
        }

        // Evita que o mesmo arquivo seja processado mais de uma vez.
        lock (_lock)
        {
            if (!_arquivosProcessando.Add(caminhoArquivo))
            {
                Debug.WriteLine(
                    $"[IGNORADO] Arquivo já está sendo processado: " +
                    $"{caminhoArquivo}");

                return;
            }
        }

        // Executa o processamento fora da thread do FileSystemWatcher.
        _ = Task.Run(() =>
        {
            try
            {
                // Aguarda o arquivo terminar de ser gravado.
                Thread.Sleep(1000);

                OrganizarComRetry(caminhoArquivo);
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

    private void OrganizarComRetry(
        string caminhoArquivo)
    {
        for (int tentativa = 1;
             tentativa <= MaxTentativas;
             tentativa++)
        {
            try
            {
                if (!File.Exists(caminhoArquivo))
                {
                    Debug.WriteLine(
                        $"[IGNORADO] Arquivo não existe mais: " +
                        $"{caminhoArquivo}");

                    return;
                }

                Debug.WriteLine(
                    $"[TENTATIVA {tentativa}/{MaxTentativas}] " +
                    $"{caminhoArquivo}");

                // A organização é responsabilidade do OrganizadorService.
                _organizadorService.OrganizarArquivo(
                    caminhoArquivo);

                Debug.WriteLine(
                    $"[OK] Arquivo organizado: {caminhoArquivo}");

                return;
            }
            catch (IOException)
            {
                Debug.WriteLine(
                    $"[AGUARDANDO] Arquivo ainda está em uso. " +
                    $"Tentativa {tentativa}/{MaxTentativas}");

                if (tentativa == MaxTentativas)
                {
                    Debug.WriteLine(
                        $"[ERRO] Não foi possível organizar: " +
                        $"{caminhoArquivo}");

                    return;
                }

                Thread.Sleep(TempoEspera);
            }
            catch (UnauthorizedAccessException)
            {
                Debug.WriteLine(
                    $"[ACESSO NEGADO] " +
                    $"Tentativa {tentativa}/{MaxTentativas}");

                if (tentativa == MaxTentativas)
                {
                    Debug.WriteLine(
                        $"[ERRO] Sem acesso ao arquivo: " +
                        $"{caminhoArquivo}");

                    return;
                }

                Thread.Sleep(TempoEspera);
            }
        }
    }

    private bool DeveIgnorarArquivo(
        string caminhoArquivo)
    {
        if (Directory.Exists(caminhoArquivo))
        {
            Debug.WriteLine(
                $"[IGNORADO] Diretório: {caminhoArquivo}");

            return true;
        }

        string nomeArquivo =
            Path.GetFileName(caminhoArquivo);

        if (nomeArquivo.EndsWith(
                ".crdownload",
                StringComparison.OrdinalIgnoreCase))
        {
            Debug.WriteLine(
                $"[IGNORADO] Arquivo temporário: {nomeArquivo}");

            return true;
        }

        if (nomeArquivo.EndsWith(
                ".part",
                StringComparison.OrdinalIgnoreCase))
        {
            Debug.WriteLine(
                $"[IGNORADO] Arquivo temporário: {nomeArquivo}");

            return true;
        }

        return false;
    }

    private void WatcherErro(
        object sender,
        ErrorEventArgs e)
    {
        Debug.WriteLine(
            $"[WATCHER ERROR] " +
            $"{e.GetException().Message}");
    }

    public void Dispose()
    {
        _watcher.EnableRaisingEvents = false;

        _watcher.Created -= ArquivoCriado;
        _watcher.Renamed -= ArquivoRenomeado;
        _watcher.Error -= WatcherErro;

        _watcher.Dispose();

        lock (_lock)
        {
            _arquivosProcessando.Clear();
        }

        Debug.WriteLine(
            "[MONITORAMENTO] Encerrado.");
    }
}