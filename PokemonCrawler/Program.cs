using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Net.Http;
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

            //the url of the page we want to test
            var url = "https://pokemondb.net/pokedex/national";
            var httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync(url);
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            var pokemons = new List<PokeDexItem>();
            var pokemonByGen =
            htmlDocument.DocumentNode.Descendants("div")
                .Where(node => node.GetAttributeValue("class", "").Equals("infocard-list infocard-list-pkmn-lg")).ToList();

            for (int i = 0; i < pokemonByGen.Count; i++)
            {
                var allPokemonsInSpecificGen = pokemonByGen[i].Descendants("div").Where(node => node.GetAttributeValue("class", "").Equals("infocard ")).ToList();

                foreach (var pokemonInGen in allPokemonsInSpecificGen)
                {
                    var pokemon = new PokeDexItem()
                    {
                        CharacterName = pokemonInGen.Descendants("a").FirstOrDefault(node => node.GetAttributeValue("class", "").Equals("ent-name")).InnerText,
                        ImageUrl = pokemonInGen.Descendants("span").FirstOrDefault(node => node.GetAttributeValue("class", "").Contains("img-fixed img-sprite")).ChildAttributes("data-src").FirstOrDefault().Value,
                        Generation = $"Generation {i + 1}",
                        UrlToStats = $"https://pokemondb.net/{pokemonInGen.Descendants("a").FirstOrDefault(node => node.GetAttributeValue("class", "").Equals("ent-name")).ChildAttributes("href").FirstOrDefault().Value}",
                        Types = pokemonInGen.Descendants("a").Where(node => node.GetAttributeValue("class", "").Contains("itype")).Select(s => s.InnerText).ToList(),

                    };
                    pokemons.Add(pokemon);
                }



            }
            Console.WriteLine("Total...."+pokemons.Count);
            Console.WriteLine("Successful....");
            Console.WriteLine("Press Enter to exit the program...");
            ConsoleKeyInfo keyinfor = Console.ReadKey(true);
            if (keyinfor.Key == ConsoleKey.Enter)
            {
                System.Environment.Exit(0);
            }

        }


    }
}

