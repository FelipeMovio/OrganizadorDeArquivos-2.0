using OrganizadorDePasta_2._0.Models;

namespace OrganizadorDePasta_2._0.Service;



using OrganizadorDePasta_2._0.Utils;
using System.IO;



public class OrganizadorService
{
    // Contém as regras utilizadas para decidir
    // em qual pasta cada arquivo será colocado.

    // readonly é um modificador aplicado a campos de uma classe ou estrutura.
    // Ele garante que o valor do campo só pode ser definido
    // na sua declaração ou dentro do construtor. Depois disso,
    // o valor não pode mais ser alterado,
    // o que ajuda a deixar o código mais seguro e previsível
    private readonly List<RegrasOrganizacao> _regras;

    public OrganizadorService()
    {
        // Inicialmente as regras são definidas diretamente no código.
        // Na Branch 02, essas regras serão configuráveis pelo usuário.
        _regras = new List<RegrasOrganizacao>
        {
            new RegrasOrganizacao
            {
                // Arquivos de imagem serão enviados para a pasta "Imagens".
                Nome = "Imagens",
                // Extensões consideradas como imagens.
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
                // Arquivos de imagem serão enviados para a pasta "Imagens".
                Nome = "Documentos",

                // Extensões consideradas como imagens.
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
                // Arquivos de imagem serão enviados para a pasta "Imagens".
                Nome = "Videos",

                // Extensões consideradas como imagens.
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
                // Arquivos de imagem serão enviados para a pasta "Imagens".
                Nome = "Compactados",

                // Extensões consideradas como imagens.
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
        // Antes de tentar acessar a pasta, verificamos se ela realmente existe.
        // Caso contrário, interrompemos a operação informando o erro
        if (!Directory.Exists(caminhoPasta))
        {
            throw new DirectoryNotFoundException(
                $"A pasta '{caminhoPasta}' não existe.");
        }

        // Obtém todos os arquivos existentes diretamente dentro da pasta.
        var arquivos = Directory.GetFiles(caminhoPasta);

        // Percorre cada arquivo encontrado e envia para o processo de organização.
        foreach (var arquivo in arquivos)
        {
            OrganizarArquivo(arquivo);
        }
    }

    private void OrganizarArquivo(string caminhoArquivo)
    {

        // Descobre a extensão do arquivo, por exemplo:
        // "foto.jpg" → ".jpg"
        var extensao = ExtensaoArquivoUtil.ObterExtensao(caminhoArquivo);

        // Procura entre as regras uma que aceite a extensão encontrada.
        var regra = EncontrarRegra(extensao);

        // Se nenhuma regra for encontrada,
        // o arquivo será enviado para a pasta "Outros".
        var nomePastaDestino = regra?.Nome ?? "Outros";

        // Obtém o diretório onde o arquivo está atualmente.
        var pastaOrigem = Path.GetDirectoryName(caminhoArquivo)!;

        // Cria o caminho da pasta de destino.
        // Exemplo:
        // C:\OrganizadorTeste + Imagens
        // → C:\OrganizadorTeste\Imagens
        var pastaDestino = Path.Combine(
            pastaOrigem,
            nomePastaDestino);

        // Cria a pasta de destino caso ela ainda não exista.
        Directory.CreateDirectory(pastaDestino);

        // Obtém somente o nome do arquivo.
        // Exemplo:
        // C:\OrganizadorTeste\foto.jpg
        // → foto.jpg
        var nomeArquivo = Path.GetFileName(caminhoArquivo);

        // Combina a pasta de destino com o nome do arquivo.
        var destino = Path.Combine(
            pastaDestino,
            nomeArquivo);

        // Move o arquivo da pasta original para a pasta de destino.
        File.Move(caminhoArquivo, destino);
    }

    private RegrasOrganizacao? EncontrarRegra(string extensao)
    {
        // Procura a primeira regra cuja lista de extensões
        // contenha a extensão do arquivo
        return _regras.FirstOrDefault(regra =>
            regra.Extensoes.Contains(extensao));
    }
}