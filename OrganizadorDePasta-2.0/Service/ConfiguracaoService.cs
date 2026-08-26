
using OrganizadorDePasta_2._0.Models;
using System.IO;
using System.Text.Json;
namespace OrganizadorDePasta_2._0.Service;

public class ConfiguracaoService
{
    public Configuracao CarregarConfiguracao()
    {
        // Ler todo conteudo do arquivo
        var json = File.ReadAllText("config.json");

        // Desserializa o JSON de volta para o objeto 
        var objeto = JsonSerializer.Deserialize<Configuracao>(json);

        //O ! é o null-forgiving operator. Estamos dizendo ao compilador
        //Neste ponto, eu estou assumindo que esse objeto não será null
        return objeto!;
    }
}
