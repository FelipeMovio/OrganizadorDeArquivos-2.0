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
| Imagens | `.jpg`, `.jpeg`, `.png`, `.gif`, `.webp` |
| Documentos | `.pdf`, `.doc`, `.docx`, `.txt` |
| Vídeos | `.mp4`, `.avi`, `.mkv`, `.mov` |
| Compactados | `.zip`, `.rar`, `.7z` |
| Outros | Arquivos sem uma regra correspondente |

As regras podem ser ajustadas no arquivo de configuração da aplicação.

## Tecnologias utilizadas

- **C#** — linguagem de programação.
- **.NET** — plataforma de desenvolvimento.
- **WPF (Windows Presentation Foundation)** — construção da interface desktop.
- **JSON** — configuração e persistência do histórico.
- **`FileSystemWatcher`** — monitoramento de alterações no sistema de arquivos.
- **`ILogger`** — logging e diagnóstico da aplicação.
- **Git e GitHub** — versionamento e gerenciamento do código-fonte.

## Organização do projeto

O projeto busca separar as responsabilidades da aplicação em componentes específicos, como:

- **Models:** representam os dados e as entidades utilizadas.
- **Services:** concentram as regras de organização, monitoramento, configuração e histórico.
- **Interface WPF:** disponibiliza os recursos da aplicação ao usuário.

A interface utiliza conceitos de `MainViewModel` e `INotifyPropertyChanged` para atualizar informações exibidas na tela.

## Como executar

1. Clone o repositório:

   ```bash
   git clone https://github.com/SEU-USUARIO/SEU-REPOSITORIO.git
   ```

2. Abra a solução ou o projeto no Visual Studio.
3. Restaure as dependências, caso necessário.
4. Compile a aplicação.
5. Execute o projeto WPF.

> Substitua o endereço do repositório no comando acima pela URL real do projeto.

## Configuração

As regras de organização são carregadas de um arquivo `config.json`.

Antes de utilizar o monitoramento, selecione ou configure uma pasta válida para acompanhar. Recomenda-se realizar os primeiros testes com uma pasta contendo arquivos de teste, evitando mover documentos importantes acidentalmente.

O histórico é armazenado em JSON no diretório de execução da aplicação, dentro da pasta `Historico`.

## Objetivos de aprendizado

Este projeto faz parte da minha jornada de aprendizado em desenvolvimento .NET e tem como objetivos:

- Aplicar conceitos de C# e orientação a objetos.
- Trabalhar com serviços e separação de responsabilidades.
- Manipular arquivos e diretórios.
- Implementar monitoramento de eventos do sistema de arquivos.
- Desenvolver uma interface desktop com WPF.
- Persistir dados em JSON.
- Aplicar logging e tratamento de exceções.
- Utilizar Git e branches para organizar o desenvolvimento por funcionalidades.

## Status do projeto

Em desenvolvimento contínuo, com funcionalidades implementadas de forma incremental para consolidar conhecimentos em C# e .NET.

## Autor

Desenvolvido por **Felipe Movio**.

Projeto pessoal de estudo e prática de desenvolvimento de software com C# e .NET.
