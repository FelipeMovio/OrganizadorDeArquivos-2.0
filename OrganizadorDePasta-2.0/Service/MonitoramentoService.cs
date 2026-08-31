using System.Diagnostics;
using System.IO;

namespace OrganizadorDePasta_2._0.Service;

public class MonitoramentoService : IDisposable
{
    // Responsável por observar alterações na pasta.
    private readonly FileSystemWatcher _watcher;

    // Serviço que contém a lógica de organização dos arquivos.
    // O MonitoramentoService não organiza o arquivo diretamente.
    // Ele apenas detecta o arquivo e delega essa responsabilidade
    // para o OrganizadorService.
    private readonly OrganizadorService _organizadorService;

    // Guarda os arquivos que já estão sendo processados.
    //
    // Isso evita que dois eventos diferentes tentem organizar
    // o mesmo arquivo ao mesmo tempo.
    private readonly HashSet<string> _arquivosProcessando = new();

    // Objeto utilizado para controlar o acesso ao HashSet.
    //
    // Como os eventos do FileSystemWatcher podem ocorrer
    // em threads diferentes, precisamos proteger o acesso
    // à coleção.
    private readonly object _lock = new();

    // Quantidade máxima de tentativas para organizar um arquivo.
    private const int MaxTentativas = 10;

    // Tempo de espera entre uma tentativa e outra.
    private const int TempoEspera = 1000;

    // Pasta que será monitorada.
    //
    // Por enquanto está fixa porque estamos desenvolvendo
    // a funcionalidade.
    //
    // Em uma fase futura podemos colocar isso em configuração.
    private const string CaminhoPasta =
        @"C:\Users\felip\Downloads";


    public MonitoramentoService(
        OrganizadorService organizadorService)
    {
        // Guarda a referência do serviço responsável
        // pela organização dos arquivos.
        _organizadorService = organizadorService;


        // Cria o FileSystemWatcher.
        _watcher = new FileSystemWatcher
        {
            // Define qual pasta será monitorada.
            Path = CaminhoPasta,

            // Define quais alterações queremos observar.
            //
            // FileName:
            // Detecta criação/renomeação de arquivos.
            //
            // LastWrite:
            // Detecta alterações no conteúdo.
            //
            // Size:
            // Detecta alterações no tamanho.
            NotifyFilter =
                NotifyFilters.FileName |
                NotifyFilters.LastWrite |
                NotifyFilters.Size
        };


        // Quando um novo arquivo aparecer,
        // o método ArquivoCriado será executado.
        _watcher.Created += ArquivoCriado;


        // Quando um arquivo for renomeado,
        // o método ArquivoRenomeado será executado.
        //
        // Isso é importante para downloads de navegadores,
        // que podem passar de:
        //
        // arquivo.zip.crdownload
        //
        // para:
        //
        // arquivo.zip
        _watcher.Renamed += ArquivoRenomeado;


        // Caso aconteça algum erro interno no watcher,
        // o método WatcherErro será chamado.
        _watcher.Error += WatcherErro;


        // Finalmente ativamos o monitoramento.
        _watcher.EnableRaisingEvents = true;


        Debug.WriteLine(
            $"[MONITORAMENTO] Iniciado: {CaminhoPasta}");
    }


    // ============================================================
    // EVENTO CREATED
    // ============================================================

    private void ArquivoCriado(
        object sender,
        FileSystemEventArgs e)
    {
        Debug.WriteLine(
            $"[CREATED] {e.FullPath}");


        // Verifica se devemos ignorar esse arquivo.
        //
        // Por exemplo:
        //
        // arquivo.zip.crdownload
        //
        // não deve ser organizado enquanto o download ainda
        // está acontecendo.
        if (DeveIgnorarArquivo(e.FullPath))
        {
            return;
        }


        // Se for um arquivo válido,
        // inicia o processo de organização.
        ProcessarArquivo(e.FullPath);
    }


    // ============================================================
    // EVENTO RENAMED
    // ============================================================

    private void ArquivoRenomeado(
        object sender,
        RenamedEventArgs e)
    {
        Debug.WriteLine(
            $"[RENAMED] {e.OldFullPath} -> {e.FullPath}");


        // Verificamos se o arquivo antigo era um arquivo
        // temporário de download do Chrome.
        //
        // Exemplo:
        //
        // arquivo.zip.crdownload
        //
        // virou:
        //
        // arquivo.zip
        if (e.OldFullPath.EndsWith(
                ".crdownload",
                StringComparison.OrdinalIgnoreCase))
        {
            Debug.WriteLine(
                $"[DOWNLOAD] Download concluído: {e.FullPath}");

            ProcessarArquivo(e.FullPath);

            return;
        }


        // Alguns programas utilizam a extensão .part
        // para arquivos temporários.
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


    // ============================================================
    // PROCESSAMENTO
    // ============================================================

    private void ProcessarArquivo(
        string caminhoArquivo)
    {
        // Garante que não estamos tentando processar
        // um diretório.
        if (Directory.Exists(caminhoArquivo))
        {
            return;
        }


        // Como os eventos podem acontecer simultaneamente,
        // protegemos o acesso ao HashSet com lock.
        lock (_lock)
        {
            // Add retorna false se o arquivo já estiver
            // dentro da coleção.
            //
            // Isso impede processamento duplicado.
            if (!_arquivosProcessando.Add(caminhoArquivo))
            {
                Debug.WriteLine(
                    $"[IGNORADO] Arquivo já está sendo processado: " +
                    $"{caminhoArquivo}");

                return;
            }
        }


        // Executamos o processamento em outra thread.
        //
        // Isso evita bloquear a thread que está recebendo
        // os eventos do FileSystemWatcher.
        _ = Task.Run(() =>
        {
            try
            {
                // Pequena espera para dar tempo ao navegador
                // ou outro programa de terminar de escrever
                // o arquivo.
                Thread.Sleep(1000);


                // Tenta organizar o arquivo.
                OrganizarComRetry(caminhoArquivo);
            }
            finally
            {
                // Quando terminar o processamento,
                // removemos o arquivo da lista.
                lock (_lock)
                {
                    _arquivosProcessando.Remove(caminhoArquivo);
                }
            }
        });
    }


    // ============================================================
    // RETRY
    // ============================================================

    private void OrganizarComRetry(
        string caminhoArquivo)
    {
        // Tentamos organizar o arquivo várias vezes.
        //
        // Isso resolve situações onde o arquivo ainda está
        // sendo utilizado pelo navegador.
        for (int tentativa = 1;
             tentativa <= MaxTentativas;
             tentativa++)
        {
            try
            {
                // Verifica se o arquivo ainda existe.
                //
                // Pode acontecer de outro processo remover
                // ou mover o arquivo.
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


                // Aqui está a parte mais importante:
                //
                // O MonitoramentoService não sabe como organizar
                // um arquivo.
                //
                // Ele simplesmente delega essa responsabilidade
                // para o OrganizadorService.
                _organizadorService.OrganizarArquivo(
                    caminhoArquivo);


                Debug.WriteLine(
                    $"[OK] Arquivo organizado: {caminhoArquivo}");

                return;
            }
            catch (IOException)
            {
                // IOException pode acontecer quando o arquivo
                // ainda está sendo utilizado por outro programa.
                Debug.WriteLine(
                    $"[AGUARDANDO] Arquivo ainda está em uso. " +
                    $"Tentativa {tentativa}/{MaxTentativas}");


                // Se chegamos à última tentativa,
                // desistimos.
                if (tentativa == MaxTentativas)
                {
                    Debug.WriteLine(
                        $"[ERRO] Não foi possível organizar: " +
                        $"{caminhoArquivo}");

                    return;
                }


                // Aguarda antes de tentar novamente.
                Thread.Sleep(TempoEspera);
            }
            catch (UnauthorizedAccessException)
            {
                // Pode acontecer caso o arquivo ou pasta
                // não permita acesso.
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


                // Aguarda antes da próxima tentativa.
                Thread.Sleep(TempoEspera);
            }
        }
    }


    // ============================================================
    // FILTRO DE ARQUIVOS
    // ============================================================

    private bool DeveIgnorarArquivo(
        string caminhoArquivo)
    {
        // Ignora diretórios.
        //
        // Queremos trabalhar apenas com arquivos.
        if (Directory.Exists(caminhoArquivo))
        {
            Debug.WriteLine(
                $"[IGNORADO] Diretório: {caminhoArquivo}");

            return true;
        }


        // Obtém apenas o nome do arquivo.
        //
        // Exemplo:
        //
        // C:\Downloads\arquivo.zip.crdownload
        //
        // vira:
        //
        // arquivo.zip.crdownload
        string nomeArquivo =
            Path.GetFileName(caminhoArquivo);


        // Ignora arquivos temporários do Chrome.
        if (nomeArquivo.EndsWith(
                ".crdownload",
                StringComparison.OrdinalIgnoreCase))
        {
            Debug.WriteLine(
                $"[IGNORADO] Arquivo temporário: {nomeArquivo}");

            return true;
        }


        // Ignora arquivos temporários .part.
        if (nomeArquivo.EndsWith(
                ".part",
                StringComparison.OrdinalIgnoreCase))
        {
            Debug.WriteLine(
                $"[IGNORADO] Arquivo temporário: {nomeArquivo}");

            return true;
        }


        // Se chegou até aqui,
        // o arquivo pode ser processado.
        return false;
    }


    // ============================================================
    // ERRO DO FILESYSTEMWATCHER
    // ============================================================

    private void WatcherErro(
        object sender,
        ErrorEventArgs e)
    {
        Debug.WriteLine(
            $"[WATCHER ERROR] " +
            $"{e.GetException().Message}");
    }


    // ============================================================
    // FINALIZAÇÃO
    // ============================================================

    public void Dispose()
    {
        // Para de gerar eventos.
        _watcher.EnableRaisingEvents = false;


        // Remove os eventos registrados.
        _watcher.Created -= ArquivoCriado;
        _watcher.Renamed -= ArquivoRenomeado;
        _watcher.Error -= WatcherErro;


        // Libera os recursos utilizados pelo watcher.
        _watcher.Dispose();


        // Limpa a lista de arquivos em processamento.
        lock (_lock)
        {
            _arquivosProcessando.Clear();
        }


        Debug.WriteLine(
            "[MONITORAMENTO] Encerrado.");
    }
}