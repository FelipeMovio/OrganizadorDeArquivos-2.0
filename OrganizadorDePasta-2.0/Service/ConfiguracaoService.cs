
using OrganizadorDePasta_2._0.Models;
using System.IO;
using System.Text.Json;
namespace OrganizadorDePasta_2._0.Service;

public class ConfiguracaoService
{
    public Configuracao CarregarConfiguracao()
    {
        // Obtém o diretório onde a aplicação está sendo executada.
        var caminhoConfig = Path.Combine(
            AppContext.BaseDirectory,
            "config.json");

        // Lê todo o conteúdo do arquivo JSON.
        var json = File.ReadAllText(caminhoConfig);

        // Converte o JSON para o objeto Configuracao.
        var objeto = JsonSerializer.Deserialize<Configuracao>(
            json,
            new JsonSerializerOptions
            {
                // Permite "regras" no JSON corresponder a "Regras" no C#.
                PropertyNameCaseInsensitive = true
            });

        //O ! é o null-forgiving operator. Estamos dizendo ao compilador
        //Neste ponto, eu estou assumindo que esse objeto não será null
        return objeto!;
    }
}
