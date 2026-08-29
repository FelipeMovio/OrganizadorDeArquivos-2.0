
using System.IO;


namespace OrganizadorDePasta_2._0.Service;

public class MonitoramentoService
{
    private readonly FileSystemWatcher _watcher;
                                       // ver/monitorar

    public MonitoramentoService(OrganizadorService organizadorService)
    {
        _watcher = new FileSystemWatcher();

        _watcher.Created += (sender, e) =>
        {
            Console.WriteLine($"Novo arquivo detectado: {e.FullPath}");
        };
    }
}
