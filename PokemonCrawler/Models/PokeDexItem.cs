using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonCrawler.Models
{
    [DebuggerDisplay("{CharacterName},{UrlToStats}")]
    class PokeDexItem
    {
        public string CharacterName { get; set; }
        public string ImageUrl { get; set; }
        public string UrlToStats { get; set; }
        public string Generation { get; set; }
        public decimal Weight { get; set; }
        public int Number { get; set; }
        public string description { get; set; }
        public List<string> Types { get; set; }
    }
}
