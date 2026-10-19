using OrganizadorDePasta_2._0.Models;

namespace OrganizadorDePasta_2._0.Service;



using OrganizadorDePasta_2._0.Utils;
using System.IO;



public class OrganizadorService
{
    private readonly List<RegrasOrganizacao> _regras;
    private readonly HistoricoService _historicoService;

    public OrganizadorService(
        Configuracao configuracao,
        HistoricoService historicoService)
    {
        _regras = configuracao.Regras;
        _historicoService = historicoService;
    }

    public int OrganizarPasta(string caminhoPasta)
    {
        if (!Directory.Exists(caminhoPasta))
        {
            throw new DirectoryNotFoundException(
                $"A pasta '{caminhoPasta}' não existe.");
        }

        var arquivos = Directory.GetFiles(caminhoPasta);

        int quantidadeOrganizada = 0;

        foreach (var arquivo in arquivos)
        {
            OrganizarArquivo(arquivo);

            quantidadeOrganizada++;
        }

        return quantidadeOrganizada;
    }




    public void OrganizarArquivo(string caminhoArquivo)
    {
        var extensao =
            ExtensaoArquivoUtil.ObterExtensao(caminhoArquivo);

        var regra = EncontrarRegra(extensao);

        var nomePastaDestino = regra?.Nome ?? "Outros";

        var pastaOrigem =
            Path.GetDirectoryName(caminhoArquivo)!;

        var pastaDestino = Path.Combine(
            pastaOrigem,
            nomePastaDestino);

        Directory.CreateDirectory(pastaDestino);

        var nomeArquivo = Path.GetFileName(caminhoArquivo);

        var destino = Path.Combine(
            pastaDestino,
            nomeArquivo);

        destino = ObterCaminhoDisponivel(destino);

        // Primeiro, movimenta o arquivo.
        File.Move(caminhoArquivo, destino);

        // Depois, registra o resultado da movimentação.
        var registro = new HistoricoArquivo
        {
            NomeArquivo = nomeArquivo,
            CaminhoOrigem = caminhoArquivo,
            CaminhoDestino = destino,
            DataHora = DateTime.Now,
            Sucesso = true,
            Mensagem = "Arquivo organizado com sucesso."
        };

        bool registrado = _historicoService.TentarRegistrar(
            registro,
            out string? erro);

        if (!registrado)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[HISTÓRICO] Não foi possível salvar o registro: {erro}");
        }
    }




    public void RegistrarFalha(
        string caminhoArquivo,
        string mensagem)
    {
        var registro = new HistoricoArquivo
        {
            NomeArquivo = Path.GetFileName(caminhoArquivo),
            CaminhoOrigem = caminhoArquivo,
            CaminhoDestino = null,
            DataHora = DateTime.Now,
            Sucesso = false,
            Mensagem = mensagem
        };

        bool registrado = _historicoService.TentarRegistrar(
            registro,
            out string? erro);

        if (!registrado)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[HISTÓRICO] Não foi possível salvar o registro de falha: {erro}");
        }
    }




    private RegrasOrganizacao? EncontrarRegra(string extensao)
    {
        // Procura a primeira regra cuja lista de extensões
        // contenha a extensão do arquivo
        return _regras.FirstOrDefault(regra =>
            regra.Extensoes.Contains(extensao));
    }
   

    private string ObterCaminhoDisponivel(string caminhoDestino)
    {
        if (!File.Exists(caminhoDestino))
        {
            return caminhoDestino;
        }

        var pasta = Path.GetDirectoryName(caminhoDestino)!;
        var nomeSemExtensao = Path.GetFileNameWithoutExtension(caminhoDestino);
        var extensao = Path.GetExtension(caminhoDestino);

        int contador = 1;

        while (true)
        {
            var novoNome = $"{nomeSemExtensao} ({contador}){extensao}";

            var novoCaminho = Path.Combine(
                pasta,
                novoNome);

            if (!File.Exists(novoCaminho))
            {
                return novoCaminho;
            }

            contador++;
        }
    }
}