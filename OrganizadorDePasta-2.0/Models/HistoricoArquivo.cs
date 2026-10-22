
namespace OrganizadorDePasta_2._0.Models;

public class HistoricoArquivo
{
    public int Id { get; set; }
    public string NomeArquivo { get; set; } = string.Empty;

    public string CaminhoOrigem { get; set; } = string.Empty;

    public string? CaminhoDestino { get; set; }

    public DateTime DataHora { get; set; }

    public bool Sucesso { get; set; }

    public string Mensagem { get; set; } = string.Empty;
}
