using OrganizadorDePasta_2._0.Models;
using OrganizadorDePasta_2._0.Models;
using OrganizadorDePasta_2._0.Utils;

namespace OrganizadorDePasta_2._0.Service;



using OrganizadorDePasta_2._0.Utils;
using System.IO;



public class OrganizadorService
{
    private readonly List<RegrasOrganizacao> _regras;

    public OrganizadorService()
    {
        _regras = new List<RegrasOrganizacao>
        {
            new RegrasOrganizacao
            {
                Nome = "Imagens",
                Extensoes = new List<string>
                {
                    ".jpg",
                    ".jpeg",
                    ".png",
                    ".gif",
                    ".webp"
                }
            },

            new RegrasOrganizacao
            {
                Nome = "Documentos",
                Extensoes = new List<string>
                {
                    ".pdf",
                    ".doc",
                    ".docx",
                    ".txt"
                }
            },

            new RegrasOrganizacao
            {
                Nome = "Videos",
                Extensoes = new List<string>
                {
                    ".mp4",
                    ".avi",
                    ".mkv",
                    ".mov"
                }
            },

            new RegrasOrganizacao
            {
                Nome = "Compactados",
                Extensoes = new List<string>
                {
                    ".zip",
                    ".rar",
                    ".7z"
                }
            }
        };
    }

    public void OrganizarPasta(string caminhoPasta)
    {
        if (!Directory.Exists(caminhoPasta))
        {
            throw new DirectoryNotFoundException(
                $"A pasta '{caminhoPasta}' não existe.");
        }

        var arquivos = Directory.GetFiles(caminhoPasta);

        foreach (var arquivo in arquivos)
        {
            OrganizarArquivo(arquivo);
        }
    }

    private void OrganizarArquivo(string caminhoArquivo)
    {
        var extensao = ExtensaoArquivoUtil.ObterExtensao(caminhoArquivo);

        var regra = EncontrarRegra(extensao);

        if (regra is null)
        {
            return;
        }

        var pastaOrigem = Path.GetDirectoryName(caminhoArquivo)!;

        var pastaDestino = Path.Combine(
            pastaOrigem,
            regra.Nome);

        Directory.CreateDirectory(pastaDestino);

        var nomeArquivo = Path.GetFileName(caminhoArquivo);

        var destino = Path.Combine(
            pastaDestino,
            nomeArquivo);

        File.Move(caminhoArquivo, destino);
    }

    private RegrasOrganizacao? EncontrarRegra(string extensao)
    {
        return _regras.FirstOrDefault(regra =>
            regra.Extensoes.Contains(extensao));
    }
}