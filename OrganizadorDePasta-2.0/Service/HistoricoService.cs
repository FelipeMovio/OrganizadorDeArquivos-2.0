
using OrganizadorDePasta_2._0.Models;
using System.IO;
using System.Text.Json;

namespace OrganizadorDePasta_2._0.Service;

public class HistoricoService
{
    private readonly List<HistoricoArquivo> _historico = new();

    private readonly string _caminhoArquivo;

    public HistoricoService()
    {
        var pastaHistorico = Path.Combine(
            AppContext.BaseDirectory,
            "Historico");

        Directory.CreateDirectory(pastaHistorico);

        _caminhoArquivo = Path.Combine(
            pastaHistorico,
            "historico.json");

        CarregarHistorico();
    }

    public void Registrar(HistoricoArquivo registro)
    {
        _historico.Add(registro);

        SalvarHistorico();
    }

    public List<HistoricoArquivo> ObterHistorico()
    {
        return _historico.ToList();
    }

    private void CarregarHistorico()
    {
        if (!File.Exists(_caminhoArquivo))
        {
            return;
        }

        var json = File.ReadAllText(_caminhoArquivo);

        var historico = JsonSerializer.Deserialize<List<HistoricoArquivo>>(
            json);

        if (historico is not null)
        {
            _historico.AddRange(historico);
        }
    }

    private void SalvarHistorico()
    {
        var opcoes = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        var json = JsonSerializer.Serialize(
            _historico,
            opcoes);

        File.WriteAllText(_caminhoArquivo, json);
    }
}
