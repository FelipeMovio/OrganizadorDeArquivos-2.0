
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
            _organizadorService.OrganizarArquivo(e.FullPath);
        };

        _watcher.EnableRaisingEvents = true;
    }
}
