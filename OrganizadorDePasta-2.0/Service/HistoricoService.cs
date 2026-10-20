
using OrganizadorDePasta_2._0.Models;
using System.IO;
using System.Text.Json;

namespace OrganizadorDePasta_2._0.Service;

public class HistoricoService
{
    private readonly List<HistoricoArquivo> _historico = new();
    private readonly string _caminhoArquivo;
    private readonly object _lock = new();

    private bool _persistenciaDisponivel = true;

    public string? AvisoCarregamento { get; private set; }

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

            if (!_persistenciaDisponivel)
            {
                erro = "A persistência do histórico está indisponível. "
                     + "O registro foi mantido apenas em memória.";

                return false;
            }

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

        string json;

        try
        {
            json = File.ReadAllText(_caminhoArquivo);

            var historico =
                JsonSerializer.Deserialize<List<HistoricoArquivo>>(
                    json);

            if (historico == null)
            {
                throw new JsonException(
                    "O conteúdo do histórico não representa uma lista válida.");
            }

            _historico.AddRange(historico);
        }
        catch (JsonException ex)
        {
            PreservarHistoricoCorrompido(ex);
        }
    }

    private void PreservarHistoricoCorrompido(
        JsonException erroJson)
    {
        var backup = _caminhoArquivo
            + $".corrompido-{DateTime.Now:yyyyMMdd-HHmmss-fff}.bak";

        try
        {
            File.Move(_caminhoArquivo, backup);

            AvisoCarregamento =
                "O histórico contém JSON inválido e não pôde ser carregado. "
                + $"O arquivo original foi preservado em: {backup}. "
                + "A aplicação iniciará com um histórico vazio.";
        }
        catch (IOException ex)
        {
            _persistenciaDisponivel = false;

            AvisoCarregamento =
                "O histórico contém JSON inválido e não pôde ser carregado. "
                + "Também não foi possível preservar o arquivo original. "
                + "A persistência foi desativada para evitar sobrescrevê-lo. "
                + $"Erro de backup: {ex.Message}";
        }
        catch (UnauthorizedAccessException ex)
        {
            _persistenciaDisponivel = false;

            AvisoCarregamento =
                "O histórico contém JSON inválido e não pôde ser carregado. "
                + "Não há permissão para preservar o arquivo original. "
                + "A persistência foi desativada para evitar sobrescrevê-lo. "
                + $"Erro de acesso: {ex.Message}";
        }

        System.Diagnostics.Debug.WriteLine(
            $"[HISTÓRICO] {erroJson.Message}");
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
