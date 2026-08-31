
using System.IO;
using System.Windows;


namespace OrganizadorDePasta_2._0.Service;

public class MonitoramentoService
{
    private readonly FileSystemWatcher _watcher;
                                       // ver/monitorar
                                      
    private readonly OrganizadorService _organizadorService;

    public MonitoramentoService(OrganizadorService organizadorService)
    {
        _organizadorService = organizadorService;
        _watcher = new FileSystemWatcher();

        string caminhoPasta = @"C:\Users\felip\Downloads";

        _watcher.Path = caminhoPasta;

        _watcher.Created += (sender, e) =>
        {
            //MessageBox.Show($"Novo arquivo detectado: {e.FullPath}");
            OrganizarComRetry(e.FullPath);
        };

        _watcher.EnableRaisingEvents = true;
    }

    private void OrganizarComRetry(string caminhoArquivo)
    {
        const int maxTentativas = 8;
        const int tempoEspera = 500;

        for (int tentativa = 1; tentativa <= maxTentativas; tentativa++)
        {
            try
            {
                _organizadorService.OrganizarArquivo(caminhoArquivo);

                return;
            }
            catch (IOException)
            {
                if (tentativa == maxTentativas)
                {
                    Console.WriteLine(
                        $"Não foi possível organizar o arquivo: {caminhoArquivo}");

                    return;
                }

                Thread.Sleep(tempoEspera);
            }
        }
    }
}
