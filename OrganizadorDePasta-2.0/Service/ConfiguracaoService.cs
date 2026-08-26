
using OrganizadorDePasta_2._0.Models;
using System.IO;
using System.Text.Json;
namespace OrganizadorDePasta_2._0.Service;

public class ConfiguracaoService
{
    public Configuracao CarregarConfiguracao()
    {
        // Ler todo conteudo do arquivo
        var json = File.ReadAllText
            ("D:\\repositorios\\c#\\OrganizadorDePasta-2.0\\OrganizadorDePasta-2.0\\config.json");

        // Desserializa o JSON de volta para o objeto 
        // Converte o JSON para o objeto Configuracao.
        // PropertyNameCaseInsensitive permite que:
        // "regras" seja associado a "Regras"
        // "nome" seja associado a "Nome"
        // "extensoes" seja associado a "Extensoes"
        var objeto = JsonSerializer.Deserialize<Configuracao>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        //O ! é o null-forgiving operator. Estamos dizendo ao compilador
        //Neste ponto, eu estou assumindo que esse objeto não será null
        return objeto!;
    }
}
