

namespace OrganizadorDePasta_2._0.ViewModels;

public class MainViewModel
{

    public string PastaMonitorada { get; set; } = string.Empty;

    public string Status { get; set; } =
        "🔴 Monitoramento parado";

    public int ArquivosOrganizados { get; set; }

    public string Mensagem { get; set; } =
        "Nenhuma mensagem";
}
