using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrganizadorDePasta_2._0.Utils;

public static class ExtensaoArquivoUtil
{
    // Obtém a extensão do arquivo e converte para letras minúsculas.
    // Isso evita problemas de comparação entre ".JPG" e ".jpg".
    public static string ObterExtensao(string caminhoArquivo)
    {
        return Path.GetExtension(caminhoArquivo).ToLowerInvariant();
    }
}
