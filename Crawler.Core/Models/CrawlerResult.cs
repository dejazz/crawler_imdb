using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMDB_Crawler.Crawler.Core.Models
{
    public interface ICrawlerResult
    {
        string Name { get; set; }
        int ReleaseYear { get; set; }
        string Director { get; set; }
        double AverageRating { get; set; }
        int NumberOfRatings { get; set; }
    }

    // Criação de uma classe concreta que implementa a interface ICrawlerResult
    public class CrawlerResult : ICrawlerResult
    {
        public string Name { get; set; } = string.Empty;
        public int ReleaseYear { get; set; }
        public string Director { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public int NumberOfRatings { get; set; }
    }
}
