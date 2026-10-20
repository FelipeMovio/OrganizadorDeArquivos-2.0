# Organizador de Pastas

Aplicação desktop desenvolvida em **C# e WPF** para automatizar a organização de arquivos em pastas, utilizando regras baseadas em extensões e monitoramento automático do sistema de arquivos.

O projeto foi desenvolvido com foco no aprendizado prático de .NET, aplicando conceitos de orientação a objetos, separação de responsabilidades, serviços, tratamento de exceções, logging e persistência de dados.

## Funcionalidades

- **Organização automática:** classifica arquivos de acordo com suas extensões.
- **Regras configuráveis:** permite definir categorias e extensões por meio de um arquivo JSON.
- **Monitoramento em tempo real:** detecta novos arquivos na pasta monitorada.
- **Tratamento de downloads:** considera arquivos temporários de download, como `.crdownload` e `.part`, evitando processá-los antes da conclusão.
- **Prevenção de duplicidade:** reduz o risco de processar o mesmo arquivo mais de uma vez.
- **Tratamento de conflitos de nomes:** gera nomes alternativos quando já existe um arquivo com o mesmo nome no destino.
- **Histórico de operações:** registra informações sobre os arquivos processados, incluindo origem, destino, data, resultado e mensagem.
- **Persistência em JSON:** mantém o histórico entre execuções do programa.
- **Recuperação de histórico corrompido:** tenta preservar o arquivo inválido em um backup antes de recriar o histórico.
- **Logging:** registra eventos e problemas utilizando `ILogger`.
- **Interface gráfica:** permite interagir com a aplicação por meio de uma interface WPF.

## Categorias de arquivos

A configuração inicial contempla as seguintes categorias:

| Categoria | Extensões |
|---|---|
| Imagens | `.
