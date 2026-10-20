
using Microsoft.Extensions.Logging;
using OrganizadorDePasta_2._0.Models;
using OrganizadorDePasta_2._0.Utils;
using System.IO;

namespace OrganizadorDePasta_2._0.Service;

public class OrganizadorService
{
    private readonly List<RegrasOrganizacao> _regras;
    private readonly HistoricoService _historicoService;
    private readonly ILogger<OrganizadorService> _logger;

    public OrganizadorService(
        Configuracao configuracao,
        HistoricoService historicoService,
        ILogger<OrganizadorService> logger)
    {
        _regras = configuracao.Regras;
        _historicoService = historicoService;
        _logger = logger;

        _logger.LogInformation(
            "OrganizadorService inicializado com {QuantidadeRegras} regras.",
            _regras.Count);
    }

    public int OrganizarPasta(string caminhoPasta)
    {
        if (!Directory.Exists(caminhoPasta))
        {
            _logger.LogWarning(
                "A pasta informada não existe: {CaminhoPasta}",
                caminhoPasta);

            throw new DirectoryNotFoundException(
                $"A pasta '{caminhoPasta}' não existe.");
        }

        _logger.LogInformation(
            "Iniciando organização da pasta {CaminhoPasta}.",
            caminhoPasta);

        var arquivos = Directory.GetFiles(caminhoPasta);
        int quantidadeOrganizada = 0;

        foreach (var arquivo in arquivos)
        {
            OrganizarArquivo(arquivo);
            quantidadeOrganizada++;
        }

        _logger.LogInformation(
            "Organização concluída. {Quantidade} arquivos organizados em {CaminhoPasta}.",
            quantidadeOrganizada,
            caminhoPasta);

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

        var nomeArquivo = Path.GetFileName(caminhoArquivo);

        try
        {
            Directory.CreateDirectory(pastaDestino);

            var destino = Path.Combine(
                pastaDestino,
                nomeArquivo);

            destino = ObterCaminhoDisponivel(destino);

            // A exceção é propagada para permitir as tentativas
            // de repetição realizadas pelo MonitoramentoService.
            File.Move(caminhoArquivo, destino);

            _logger.LogInformation(
                "Arquivo {NomeArquivo} movido de {Origem} para {Destino}.",
                nomeArquivo,
                caminhoArquivo,
                destino);

            var registro = new HistoricoArquivo
            {
                NomeArquivo = nomeArquivo,
                CaminhoOrigem = caminhoArquivo,
                CaminhoDestino = destino,
                DataHora = DateTime.Now,
                Sucesso = true,
                Mensagem = "Arquivo organizado com sucesso."
            };

            RegistrarHistorico(registro);
        }
        catch (IOException ex)
        {
            _logger.LogError(
                ex,
                "Erro de entrada/saída ao organizar o arquivo {CaminhoArquivo}.",
                caminhoArquivo);

            throw;
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(
                ex,
                "Acesso negado ao organizar o arquivo {CaminhoArquivo}.",
                caminhoArquivo);

            throw;
        }
    }

    public void RegistrarFalha(
        string caminhoArquivo,
        string mensagem)
    {
        _logger.LogWarning(
            "Registrando falha na organização de {CaminhoArquivo}: {Mensagem}",
            caminhoArquivo,
            mensagem);

        var registro = new HistoricoArquivo
        {
            NomeArquivo = Path.GetFileName(caminhoArquivo),
            CaminhoOrigem = caminhoArquivo,
            CaminhoDestino = null,
            DataHora = DateTime.Now,
            Sucesso = false,
            Mensagem = mensagem
        };

        RegistrarHistorico(registro);
    }

    private void RegistrarHistorico(HistoricoArquivo registro)
    {
        bool registrado = _historicoService.TentarRegistrar(
            registro,
            out string? erro);

        if (!registrado)
        {
            _logger.LogWarning(
                "Não foi possível persistir o histórico do arquivo {NomeArquivo}: {Erro}",
                registro.NomeArquivo,
                erro);
        }
        else
        {
            _logger.LogDebug(
                "Registro de histórico salvo para {NomeArquivo}.",
                registro.NomeArquivo);
        }
    }

    private RegrasOrganizacao? EncontrarRegra(string extensao)
    {
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
        var nomeSemExtensao =
            Path.GetFileNameWithoutExtension(caminhoDestino);
        var extensao = Path.GetExtension(caminhoDestino);

        int contador = 1;

        while (true)
        {
            var novoNome =
                $"{nomeSemExtensao} ({contador}){extensao}";

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
