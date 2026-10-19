
using OrganizadorDePasta_2._0.Models;
using System.IO;
using System.Text.Json;

namespace OrganizadorDePasta_2._0.Service;

public class HistoricoService
{
    private readonly List<HistoricoArquivo> _historico = new();
    private readonly string _caminhoArquivo;
    private readonly object _lock = new();

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

    public bool TentarRegistrar(
        HistoricoArquivo registro,
        out string? erro)
    {
        lock (_lock)
        {
            _historico.Add(registro);

            try
            {
                SalvarHistorico();

                erro = null;
                return true;
            }
            catch (IOException ex)
            {
                erro = ex.Message;
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                erro = ex.Message;
                return false;
            }
        }
    }

    public List<HistoricoArquivo> ObterHistorico()
    {
        lock (_lock)
        {
            return _historico.ToList();
        }
    }

    private void CarregarHistorico()
    {
        if (!File.Exists(_caminhoArquivo))
        {
            return;
        }

        var json = File.ReadAllText(_caminhoArquivo);

        var historico =
            JsonSerializer.Deserialize<List<HistoricoArquivo>>(json);

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
