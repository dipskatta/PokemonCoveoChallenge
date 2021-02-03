using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Net.Http;
using System.Text.RegularExpressions;
using Coveo.Connectors.Utilities.PlatformSdk;
using Coveo.Connectors.Utilities.PlatformSdk.Config;
using Coveo.Connectors.Utilities.PlatformSdk.Model.Document;
using Newtonsoft.Json.Linq;
using PokemonCrawler.Models;

namespace PokemonCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            startPokemonCrawler();
            Console.ReadLine();
        }
        private static async Task startPokemonCrawler()
        {

            var htmlDocument = CreateCrawler("https://pokemondb.net/pokedex/national").Result;

            var pokemonByGen =
            htmlDocument.DocumentNode.Descendants("div")
                .Where(node => node.GetAttributeValue("class", "").Equals("infocard-list infocard-list-pkmn-lg")).ToList();

            for (int i = 0; i < pokemonByGen.Count; i++)
            {
                var pokemons = new List<PushDocument>();

                var allPokemonsInSpecificGen = pokemonByGen[i].Descendants("div").Where(node => node.GetAttributeValue("class", "").Equals("infocard ")).ToList();

                foreach (var pokemonInGen in allPokemonsInSpecificGen)
                {
                    var pokemonUrl =
                        $"https://pokemondb.net/{pokemonInGen.Descendants("a").FirstOrDefault(node => node.GetAttributeValue("class", "").Equals("ent-name")).ChildAttributes("href").FirstOrDefault().Value}";

                    var pokemonDetailsHtmlDocument = CreateCrawler(pokemonUrl).Result;
                    var description = pokemonDetailsHtmlDocument.DocumentNode.Descendants("div").FirstOrDefault(node =>
                        node.GetAttributeValue("class", "").Equals("grid-col span-md-6 span-lg-8")).InnerHtml;
                    var details = pokemonDetailsHtmlDocument.DocumentNode.Descendants("table").Where(node =>
                        node.GetAttributeValue("class", "").Equals("vitals-table")).ToList();
                    var pokedexDataTable = details.FirstOrDefault().Descendants("td").ToList();

                    var pokemon = new PokeDexItem()
                    {
                        CharacterName = pokemonInGen.Descendants("a").FirstOrDefault(node => node.GetAttributeValue("class", "").Equals("ent-name")).InnerText,
                        ImageUrl = pokemonInGen.Descendants("span").FirstOrDefault(node => node.GetAttributeValue("class", "").Contains("img-fixed img-sprite")).ChildAttributes("data-src").FirstOrDefault().Value,
                        Generation = $"Generation {i + 1}",
                        UrlToStats = pokemonUrl,
                        Types = pokemonInGen.Descendants("a").Where(node => node.GetAttributeValue("class", "").Contains("itype")).Select(s => s.InnerText).ToList(),
                        Weight = decimal.Parse(pokedexDataTable[4].InnerText.Replace("&nbsp;"," ").Split(' ')[0]),
                        description = RemoveHtmlTags(description),
                        Number = Int32.Parse(pokedexDataTable[0].InnerText)
                    };

                    var documentToAdd = new PushDocument(pokemon.UrlToStats)
                    {
                        ClickableUri = pokemon.UrlToStats,
                        ModifiedDate = DateTime.UtcNow,
                        Metadata =
                        {
                            new KeyValuePair<string, JToken>("charactername",pokemon.CharacterName), 
                            new KeyValuePair<string, JToken>("ImageUrl",pokemon.ImageUrl), 
                            new KeyValuePair<string, JToken>("generationstring",pokemon.Generation), 
                            new KeyValuePair<string, JToken>("UrlToStats",pokemon.UrlToStats), 
                            new KeyValuePair<string, JToken>("Types",string.Join(";",pokemon.Types)), 
                            new KeyValuePair<string, JToken>("description", pokemon.description), 
                            new KeyValuePair<string, JToken>("pokemonnumber", pokemon.Number), 
                            new KeyValuePair<string, JToken>("pokemonweight", pokemon.Weight) 
                        }
                    };
                    pokemons.Add(documentToAdd);
                }

                PushToSource(pokemons);

            }
            Console.WriteLine("Successful....");
            Console.WriteLine("Press Enter to exit the program...");
            ConsoleKeyInfo keyinfor = Console.ReadKey(true);
            if (keyinfor.Key == ConsoleKey.Enter)
            {
                System.Environment.Exit(0);
            }

        }

        private static void PushToSource(List<PushDocument> documents)
        {
            string apiKey = "xxf658e848-24d4-4424-8830-edb1b82637da";
            string organizationId = "coveopokemonchallengepcm0rz3k";
            string sourceId = "coveopokemonchallengepcm0rz3k-steno3qxuvfyenkroqzvxikot4";
            //string apiKey = "xxfe408680-a663-41f1-a351-e5ec3e8d09ba";
            //string organizationId = "coveopokemonchallengepcm0rz3k";
            //string sourceId = "coveopokemonchallengepcm0rz3k-vs73qz3k5fu432ktvy7a65knxm";
            ICoveoPlatformConfig config = new CoveoPlatformConfig(apiKey, organizationId);
            ICoveoPlatformClient client = new CoveoPlatformClient(config);
            client.DocumentManager.AddOrUpdateDocuments(sourceId, documents, null);
        }

        private static async Task<HtmlDocument> CreateCrawler(string url)
        {
            var httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync(url);
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
            return htmlDocument;
        }

        private static string RemoveHtmlTags(string text)
        {
            return Regex.Replace(text, @"<[^>]*>", string.Empty);
        }
    }
}

