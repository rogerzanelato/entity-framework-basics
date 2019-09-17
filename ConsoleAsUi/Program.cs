using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SamuraiApp.Data;
using SamuraiApp.Domain;

namespace ConsoleAsUi
{
    class Program
    {
        public static SamuraiContext _context = new SamuraiContext();
        static void Main(string[] args)
        {
            //InsertSamurai();
            //InsertMultipleSamurais();
            //SimpleSamuraiQuery();
            //MoreQueries();
            //RetrieveAndUpdateSamurai();
            //RetrieveAndUpdateMultipleSamurais();
            //MultipleDatabaseOperations();
            //InsertBattle();
            //QueryAndUpdateBattle_Disconnected();
            //QueryAndUpdateWithDifferentMethods();
            //AddManySamurais();x 
            //DeleteMany();
            //DeleteUsingId(2);

            // Module: Querying and Saving Related Data
            //InsertNewPkFkGraph();
            //AddChildToExistingObjectWhileTracked();
            //AddChildToExistingObjectWhileNotTracked(3);
            //EagerLoadSamuraiWithQuotes();
            //LazyLoadSamuraiWithQuotes(); // LazyLoad não está adicionado ainda ao EFCore mas estará num futuro imprevisto
            //ProjectSomeProperties();
            //FilteringWithRelatedData();
            //ModifyingRelatedDataWhenTracked();
            ModifyingRelatedDataWhenNotTracked();
        }

        private static void InsertSamurai()
        {
            var samurai = new Samurai { Name = "Julie" };

            using (var context = new SamuraiContext())
            {
                /**
                 * Estamos utilizando aqui o próprio atributo Samurais do Context,
                 * mas poderíamos apenas chamá-lo diretamente e deixar o EFCore deduzir seu tipo. Ex: context.add(samurai);
                 */
                context.Samurais.Add(samurai);
                context.SaveChanges();
            }
        }

        private static void InsertMultipleSamurais()
        {
            var samurai = new Samurai { Name = "Julie" };
            var samuraiJohnWick = new Samurai { Name = "John Wick" };

            using (var context = new SamuraiContext())
            {
                // Poderia também ser por uma List<Samurai>, ao invés de passar por parâmetros
                context.Samurais.AddRange(samurai, samuraiJohnWick);
                context.SaveChanges();
            }
        }

        private static void SimpleSamuraiQuery()
        {
            using (var context = new SamuraiContext())
            {
                var samurais = context.Samurais.ToList();
            }
        }
        private static void MoreQueries()
        {
            var name = "Julie";

            // Mesma coisa que o abaixo. Porém, o outro é mais eficiente
            Console.WriteLine("\nWhere -> FirstOrDefault");
            var samurais = _context.Samurais.Where(samurai => samurai.Name == name).FirstOrDefault();

            Console.WriteLine("\nFirstOrDefault (mais eficiente)");
            var samuraisMaisEficiente = _context.Samurais.FirstOrDefault(samurai => samurai.Name == name);

            Console.WriteLine("\nEncontrarPorId");
            var samuraisPorId = _context.Samurais.Find(2);

            Console.WriteLine("\nFind Novamente com Mesmo ID (não faz query no banco, tipo Doctrine)");
            var samuraisPorIdRepetido = _context.Samurais.Find(2);

            Console.WriteLine("\nLike");
            var samuraisLike = _context.Samurais.Where(samurai => EF.Functions.Like(samurai.Name, "J%")).ToList();
        }
        private static void RetrieveAndUpdateSamurai()
        {
            var samurai = _context.Samurais.FirstOrDefault();
            samurai.Name += "-San";
            _context.SaveChanges();
        }

        private static void RetrieveAndUpdateMultipleSamurais()
        {
            var samurais = _context.Samurais.ToList();
            samurais.ForEach(samurai => samurai.Name += "-San");
            _context.SaveChanges();
        }

        private static void MultipleDatabaseOperations()
        {
            var samurai = _context.Samurais.FirstOrDefault();
            samurai.Name += "Hiro";

            _context.Samurais.Add(new Samurai { Name = "Kikuchiyo" });

            _context.SaveChanges();
        }

        private static void InsertBattle()
        {
            _context.Battles.Add(new Battle
            {
                Name = "Battle of Okehazama",
                StartDate = new DateTime(1560, 05, 01),
                EndDate = new DateTime(1560, 06, 15)
            });

            _context.SaveChanges();
        }

        private static void QueryAndUpdateBattle_Disconnected()
        {
            var sep = "-----------------------";
            Console.WriteLine(sep + "Selecting...");
            var battle = _context.Battles.FirstOrDefault();
            battle.EndDate = new DateTime(1560, 06, 30);

            using (var newContextInstance = new SamuraiContext())
            {
                Console.WriteLine(sep + "Updating new Context...");
                newContextInstance.Battles.Update(battle);
                newContextInstance.SaveChanges();
            }

            Console.WriteLine(sep + "Updating real Context...");
            _context.Battles.Update(battle);
            _context.SaveChanges();

            // Observação: Segundo o Curso, o segundo Update deveria alterar apenas a propriedade EndDate, mas na prática ele alterou todos os campos. ¯\_(ツ)_/¯
        }

        private static void QueryAndUpdateWithDifferentMethods()
        {
            var samurai = _context.Samurais.FirstOrDefault();
            string oldName = samurai.Name;
            string nameWithSamaSufix = samurai.Name.Replace("-San", "-Sama");

            // Way 1
            Console.WriteLine("Way 1");
            samurai.Name = nameWithSamaSufix;
            _context.Samurais.Update(samurai);
            _context.SaveChanges();

            // Way2
            Console.WriteLine("Way 2");
            samurai.Name = oldName;
            _context.Update(samurai);
            _context.SaveChanges();

            // Way3
            Console.WriteLine("Way 3");
            samurai.Name = nameWithSamaSufix;
            _context.Update<Samurai>(samurai);
            _context.SaveChanges();

            // Observação: Os mesmos métodos possuem equivalentes para Add (adicionar
        }

        private static void AddManySamurais()
        {
            List<Samurai> samurais = new List<Samurai>();

            for (int i = 0; i < 5; i++) {
                samurais.Add(new Samurai { Name = "Samurai" + i.ToString() });
            }

            _context.Samurais.AddRange(samurais);
            _context.SaveChanges();
        }

        private static void DeleteMany()
        {
            var samurais = _context.Samurais.Where(samurai => samurai.Name.Contains("Samurai"));
            _context.Samurais.RemoveRange(samurais);
            _context.SaveChanges();
        }
        private static void DeleteWhileNotTracked()
        {
            var samuraiEntity = _context.Samurais.FirstOrDefault(samurai => samurai.Name == "Julie-Sama-SamaHiroHiro");

            using (var newContext = new SamuraiContext())
            {
                newContext.Samurais.Remove(samuraiEntity);
                newContext.SaveChanges();
            }
        }

        private static void DeleteUsingId(int samuraiId)
        {
            var samurai = _context.Samurais.Find(samuraiId);
            _context.Remove(samurai);
            _context.SaveChanges();
        }

        private static void InsertNewPkFkGraph()
        {
            var samurai = new Samurai
            {
                Name = "Kambei Shimada",
                Quotes = new List<Quote>
                    {
                        new Quote {Text = "I've come to save you"},
                        new Quote {Text = "I'm back"}
                    }
            };
            _context.Samurais.Add(samurai);
            _context.SaveChanges();
        }
        
        private static void AddChildToExistingObjectWhileTracked()
        {
            var samurai = _context.Samurais.First();
            samurai.Quotes.Add(new Quote
            {
                Text = "I Bet you're happy that I've saved you!"
            });
            _context.SaveChanges();
        }

        private static void AddChildToExistingObjectWhileNotTracked(int samuraiId)
        {
            // Recomendação do Curso
            // Quando você estiver num cenário desconectado (quando você não capturou a entidade do banco). Não tente fazer macetes para o EF Core começar a rastrear
            // a entidade (utilizar o Add por exemplo, não vai funcionar!!!)
            // apenas utilize a Foreign Key. Será sempre a forma mais simples e mais fácil

            var quote = new Quote
            {
                Text = "Now that I saved you, will you feed me dinner?",
                SamuraiId = samuraiId
            };

            using (var newContext = new SamuraiContext())
            {
                newContext.Quotes.Add(quote);
                newContext.SaveChanges();
            }
        }
        private static void EagerLoadSamuraiWithQuotes()
        {
            //var samuraiWithQuotes = _context.Samurais.Include(samurai => samurai.Quotes).ToList();
            var samuraiWithQuotes = _context.Samurais.Where(samurai => samurai.Name.Contains("-San"))
                                                    .Include(samurai => samurai.Quotes)
                                                    //.ThenInclude(quote => quote.QuoteTranslation) Caso queiramos recuperar os filhos de Quote
                                                    .Include(samurai => samurai.SecretIdentity)
                                                    .FirstOrDefault();

        }

        public struct IdAndName
        {
            public IdAndName(int id, string name)
            {
                Id = id;
                Name = name;
            }

            public int Id;
            public string Name;
        }

        private static void ProjectSomeProperties()
        {
            var idsAndNames = _context.Samurais
                .Select(samurai => new IdAndName(samurai.Id, samurai.Name))
                .ToList();

            var someProperties = _context.Samurais
                .Select(samurai => new
                {
                    samurai.Id,
                    samurai.Name,
                    samurai.Quotes.Count
                })
                .ToList();

            var somePropertiesWithSomeQuotes = _context.Samurais
                .Select(samurai => new
                {
                    samurai.Id,
                    samurai.Name,
                    HappyQuotes = samurai.Quotes.Where(quote => quote.Text.Contains("happy"))
                })
                .ToList();

            var samuraisWithHappyQuotes = _context.Samurais
                .Select(samurai => new
                {
                    Samurai = samurai,
                    Quotes = samurai.Quotes.Where(quote => quote.Text.Contains("happy")).ToList()
                })
                .ToList();
        }

        private static void FilteringWithRelatedData()
        {
            var samurais = _context.Samurais
                .Where(samurai => samurai.Quotes.Any(quote => quote.Text.Contains("happy")))
                .ToList();
        }

        private static void ModifyingRelatedDataWhenTracked()
        {
            var samurai = _context.Samurais.Include(pSamurai => pSamurai.Quotes).FirstOrDefault();
            samurai.Quotes[0].Text += " Did you hear that?";
            //_context.Quotes.Remove(samurai.Quotes[2]);
            _context.SaveChanges();
        }
        private static void ModifyingRelatedDataWhenNotTracked()
        {
            var samurai = _context.Samurais.Include(pSamurai => pSamurai.Quotes).FirstOrDefault();
            var quote = samurai.Quotes[0];
            quote.Text += " Did you hear that, again?";
            
            using (var newContext = new SamuraiContext())
            {
                // Não Funciona!! O EF Core enviou para o banco um Update em TODAS as Quotes do Samurai
                //newContext.Quotes.Update(quote);

                /*
                 * Isso ocorre porque a Quote retornada no FirstOrDefault possuí uma referência à Samurai, que possuí uma referência à todas suas Quotes
                 * isso ocorre apenas porque estamos alterando uma Quote num contexto em que o EFCore não está rastreando a entidade.
                 * Dessa forma, ele acredita que todas aquelas Quotes precisavam ser alteradas
                */

                // Forma de Corrigir
                newContext.Entry(quote).State = EntityState.Modified;

                // Utilizando o método Entry, informamos diretamente ao EF Core que a Quote acima foi modificada e assim, ele alterará apenas ela.

                newContext.SaveChanges();
            }
        }

    }
}
 
 