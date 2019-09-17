# EF Core 2 Curso

Curso de introdução ao Entity Framework Core da Plural Sight.

[Curso Original](https://app.pluralsight.com/library/courses/entity-framework-core-2-getting-started)
[Repositório Original](https://github.com/julielerman/PluralsightEFCore2GettingStarted)

## Sobre o EF Core
O Entity Framework Core é a versão reescrita do Entity Framework original, com melhor desempenho e com solução para antigos problemas, capaz de ser executada em múltiplos SO. A Microsoft teria originalmente lançado o Entity Framework 7, mas ao invés disso, optaram por lançar o novo Entity Framework Core.

Apesar do Core no nome, ele não é restrito à aplicações que utilizem o `.Net Framework Core`, ele pode ser utilizado normalmente com aplicações `.Net Framework` normal, ou mesmo `.Net Standard` que tenham sua versão compatível. [Veja tabela oficial de compatibilidade](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)

Para aplicações UWP (Universal Windows Platform) a máquina de desenvolvimento deve ter o Windows 10 Fall Creators Update instalado.

## Tracking

O EFCore também funciona de maneira similar ao Hibernate e Doctrine, quando capturamos objetos, ele irá manter seu índice armazenado em memória. Dessa forma, se por exemplo utilizarmos o `Find()` em um Id de uma Entidade que já capturamos anteriormente, ele irá retornar o objeto em memória.

Ou seja, o EFCore mantêm um "rastreio" de todas as entidades com que estamos lidando.

Isso impacta a forma que ele trabalhará com as entidades. Caso a instância do EFCore não possua conhecimento de como a entidade está no banco, ao invés de alterar apenas a propriedade que foi alterada do objeto, ele irá atualizar TODAS.

## NuGet

Pacotes Instalados (na pasta Infra/Data):
- EntityFramework.Core
- EntityFramework.SqlServer (para utilizar o localdb)
- EntityFremework.Tools (para utilizar migrations/seeds entre outros)
- Microsoft.Extensions.Logging (para logar o SQL)
- Microsoft.Extensions.Logging.Console (para liberar o método AddConsole() no Builder de Log, e enviar os logs para o console)

Para o projeto principal (ou executável temporário) é necessário instalar o pacote:
- EntityFrameworkCore.Design (para utilizar as migrations)

## Migrations Tool

[Documentação](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/#install-the-tools)
Para utilizar as ferramentas de Migration do EFCore é necessário que "Startup Project" seja um executável com o EntityFramework.Design instalado. 

Package Manager Console: Tools -> NuGet Package Manager -> Package Manager Console

- `get-help entityframeworkCore`: Exibe os comandos disponíveis do EFCore
- `add-migration migration-name`: Cria o Script de migração para que o `schema` do banco de dados, seja igual ao especificado nas models.
- `update-database`: Efetua as alterações no banco de dados, de acordo com as Migrations geradas. (não recomendado para se utilizar em produção)
- `script-migration`: Gera o Script SQL das alterações. Recomendados para utilizar na hora de efetuar o build em produção.

**Como o add-migration funciona?**
Ao adicionar a primeira Migration, é possível perceber que foi criado um arquivo **SamuraiContextModelSnapshot.cs**, esse arquivo será responsável por manter um registro de como o estado das models se encontra atualmente. Através dele, ao adicionarmos uma nova Migration, o EFCore poderá dizer quais alterações precisam ser feitas para normatizar o banco de dados.

## Engenharia Reversa

Cria o `DbContext` a partir do banco de dados, atualizações no `Model` a partir do banco de dados atualmente **NÃO** é suportado, mas um dia será. 

Para utilizar **Migrations** em um projeto que foi iniciado com Engenharia Reversa, é necessário fazer algumas alterações. [Veja aqui](https://cmatskas.com/ef-core-migrations-with-existing-database-schema-and-data/).

Para efetuar a Engenharia Reversa, deve-se selecionar como **Startup Project** a pasta Model, utiliza-se  então o comando `scaffold-dbcontext`:

Exemplo:

```shell
scaffold-dbcontext -provider Microsoft.EntityFrameworkCore.SqlServer -connection "Data Source=(localdb)\\mssqllocaldb;Initial Catalog=MyLegacyDB;Trusted_Connection=True;"
```

O exemplo acima irá gerar as tabelas do banco todo.

## Mapeamento

Caso criemos o DbContext sem definir mapeamentos, o EF Core irá inferir os nomes das tabelas, relacionamentos e tipos de dados avaliando nossas classes Model a partir de suas convenções padrões.

Caso queiramos utilizar uma convenção própria, exemplo, nomenclatura, campos requerido, tipos específicos, etc.. Devemos efetuar um Mapeamento, podendo ser efetuado por `Annotations` na classe Model, ou, por métodos no `EF Core`.

[Documentação](https://docs.microsoft.com/en-us/ef/core/modeling/relational/tables)

## Batch Operations

O tamanho máximo por lotes de operações em massa é definido pelo **Database Provider**, o EF Core irá enviar os dados agrupados de acordo com essa quantidade.
Podemos alterar essa quantidade no método OnConfiguring do DbContext, respeitando as limitações do provider/db.

# LINQ Queries
Linq queries podem ser efetuadas como métodos ou como Query. É importante entrar em um consenso com a equipe sobre qual o formato desejado.

```c#
// Method
context.Samurais.ToList();

// Query
(from s in context.Samurais select s).ToList();
```

O EF Core irá tentar se adaptar ao que você está fazendo. Por exemplo:

```c#
// Irá deixar a conexão aberta com o banco até a finalização do Loop. 
foreach (var s in context.Samurais)
{
	RunSomeValidator(s.Name);
	CallSomeService(s.Id);
}

// Irá puxar tudo em memória e encerrar a conexão.
var samurais = context.Samurais.ToList();
foreach (var s in context.Samurais)
{
	RunSomeValidator(s.Name);
	CallSomeService(s.Id);
}
```

### Características
É importante lembrar algumas características dos métodos de uma Linq Query.

- Os métodos `LastOrDefault()` e `Last()` requerem que a Query possua um `OrderBy()`, caso não possua, ela irá retornar **TODOS** os resultados e retornar o último.
- Os métodos `SingleOrDefault()` e `Single()` requerem que a Query retorne apenas 1 resultado. Caso retorne mais que um, irá lançar um **Erro**.
- Os métodos `FirstOrDefault()` e `First()` por outro lado, apenas pegam o primeiro entre os resultados da Query.

Observação: Os métodos que não possuem o `OrDefault` irão retornar um erro, caso não haja nenhum resultado. Os métodos que possuem o `OrDefault` por outro lado, retorna null caso não haja resultados.
Observação²: Todos os métodos Linq possuem sua contra partida com `Async` no final, utilizados para efetuar queries de maneira Assíncrona.
Observação³: Caso seja utilizado variáveis como parâmetro de uma Linq Query, o EFCore irá automaticamente transformar esse parâmetro num placeholder (Anti-Injection \o/);


## Obs
- `Ctrl + E, C` ou `Ctrl + K, C`: Auto comentar código
- `Ctrl + E, U` ou `Ctrl + K, U`: Auto descomentar código
- `Configuration.GetConnectionString("SamuraiConnection")`: Captura a String de Conexão definida no arquivo `appsetings.json`, sendo **ConnectionStrings->>'SamuraiConnection'**.
- É possível habilitar a Migração de forma automática para que o banco se ajuste automáticamente quando estiver em produção. Mas essa técnica deve ser implementada apenas se o caso de uso permitir. [Documentação](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/#apply-migrations-at-runtime) 

