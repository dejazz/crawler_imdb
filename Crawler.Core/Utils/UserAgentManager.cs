using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler_Data_Lawer.Crawler.Core.Utils
{
    public class UserAgentManager
    {
        private readonly List<string> userAgents;
        private readonly Random random;

        public UserAgentManager()
        {
            random = new Random();

            // Lista de User-Agents
            userAgents = new List<string>
        {
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:120.0) Gecko/20100101 Firefox/120.0",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Edge/120.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 13_5_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 13_5_1) AppleWebKit/537.36 (KHTML, like Gecko) Safari/537.36",
            "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
            "Mozilla/5.0 (iPhone; CPU iPhone OS 16_6 like Mac OS X) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Mobile Safari/537.36",
            "Mozilla/5.0 (Linux; Android 13; SM-G998B) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Mobile Safari/537.36",
            "Mozilla/5.0 (Linux; Android 12; Pixel 6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Mobile Safari/537.36",
            "Mozilla/5.0 (Linux; Android 10; Nexus 5) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Mobile Safari/537.36"
        };
        }

        // Obtém um User-Agent aleatório
        public string GetRandomUserAgent()
        {
            int index = random.Next(userAgents.Count);
            return userAgents[index];
        }

        // Adiciona um novo User-Agent à lista
        public void AddUserAgent(string userAgent)
        {
            if (!string.IsNullOrWhiteSpace(userAgent) && !userAgents.Contains(userAgent))
            {
                userAgents.Add(userAgent);
            }
        }

        // Aplica um User-Agent aleatório a um HttpClient
        public void ApplyRandomUserAgent(HttpClient client)
        {
            if (client != null)
            {
                string userAgent = GetRandomUserAgent();
                client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
            }
        }
    }
}
