
using OrganizadorDePasta_2._0.Models;

namespace OrganizadorDePasta_2._0.Service;

public class HistoricoService
{
    private readonly List<HistoricoArquivo> _historico = new();

    public void Registrar(HistoricoArquivo registro)
    {
        _historico.Add(registro);
    }

    public List<HistoricoArquivo> ObterHistorico()
    {
        return _historico.ToList();
    }
}
