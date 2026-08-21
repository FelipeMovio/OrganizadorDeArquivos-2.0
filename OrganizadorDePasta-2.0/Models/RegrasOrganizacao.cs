using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrganizadorDePasta_2._0.Models;

public class RegrasOrganizacao
{
    // Nome da pasta que será criada para organizar os arquivos.
    // Exemplo: "Imagens", "Documentos", "Videos".
    public string Nome { get; set; } = string.Empty;

    // Lista de extensões que pertencem a esta regra.
    // Exemplo: ".jpg", ".png" e ".jpeg".
    public List<string> Extensoes { get; set; } = new();
}
