
using Microsoft.EntityFrameworkCore;
using OrganizadorDePasta_2._0.Models;
using OrganizadorDePasta_2._0.Repository;
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
    private readonly IDbContextFactory<OrganizadorDbContext> _dbContextFactory;
    public HistoricoService(
        IDbContextFactory<OrganizadorDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;

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

            var erros = new List<string>();

            // Tenta salvar no JSON.
            if (_persistenciaDisponivel)
            {
                try
                {
                    SalvarHistorico();
                }
                catch (IOException ex)
                {
                    erros.Add($"JSON: {ex.Message}");
                }
                catch (UnauthorizedAccessException ex)
                {
                    erros.Add($"JSON: {ex.Message}");
                }
            }
            else
            {
                erros.Add("JSON: a persistência está indisponível.");
            }

            // Tenta salvar no SQL Server independentemente do JSON.
            try
            {
                SalvarNoSqlServer(registro);
            }
            catch (Exception ex)
            {
                erros.Add($"SQL Server: {ex.Message}");
            }

            if (erros.Count > 0)
            {
                erro = string.Join(Environment.NewLine, erros);
                return false;
            }

            erro = null;
            return true;
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
    private void SalvarNoSqlServer(HistoricoArquivo registro)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        // O Id é gerado pelo SQL Server.
        registro.Id = 0;

        dbContext.HistoricoArquivos.Add(registro);

        dbContext.SaveChanges();
    }

}
