using Crawler_Data_Lawer.Crawler.Core.Utils;
using HtmlAgilityPack;
using IMDB_Crawler.Crawler.Core.Models;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using OpenQA.Selenium.DevTools;
using Crawler_Data_Lawer.Crawler.Core.Models;

namespace IMDB_Crawler.Crawler.Core
{
    public class IMDBCrawler(List<OpenQA.Selenium.Cookie> cookies, ILogger<IMDBCrawler> logger)
    {
        private readonly List<OpenQA.Selenium.Cookie> _cookies = cookies;
        private readonly ILogger<IMDBCrawler> _logger = logger;

        // Método para extrair os 250 filmes mais bem avaliados
        public async Task<List<CrawlerResult>> ExtractTop20MoviesAsync()
        {
            List<CrawlerResult> topMovies = new List<CrawlerResult>();
            string url = "https://www.imdb.com/chart/top/?ref_=nv_mv_250";
            UserAgentManager userAgentManager = new UserAgentManager();
            string _userAgent = userAgentManager.GetRandomUserAgent();

            try
            {
                _logger.LogInformation("Iniciando a extração dos 250 filmes mais bem avaliados.");

                using (var handler = new HttpClientHandler())
                {
                    handler.UseCookies = false; // Desativar cookies automáticos para configurar manualmente

                    using (var client = new HttpClient(handler))
                    {
                        // Adiciona os cookies manualmente caso existam
                        if (_cookies != null && _cookies.Any())
                        {
                            var cookieHeader = string.Join("; ", _cookies.Select(c => $"{c.Name}={c.Value}"));
                            client.DefaultRequestHeaders.Add("Cookie", cookieHeader);
                        }
                        client.DefaultRequestHeaders.UserAgent.ParseAdd(_userAgent);
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
                        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("pt-BR,en;q=0.9");

                        // Enviando a requisição para o site
                        var response = await client.GetAsync(url);
                        if (!response.IsSuccessStatusCode)
                        {
                            _logger.LogError("Erro ao obter os dados da página. Status Code: {StatusCode}", response.StatusCode);

                            return [];
                        }
                        string pageContent = await response.Content.ReadAsStringAsync();

                        // Usa o HtmlAgilityPack para processar o HTML pois o json que o frontend consome volta no fim do html de resposta
                        JObject jsonData = HTMLHelper.ExtractJsonFromScript(pageContent, "application/ld+json");
                        List<CrawlerResult> responseCrawlerResult = await ProcessMoviesAsync(jsonData);
                        return responseCrawlerResult;

                    }
                }
            }
            catch (HttpRequestException e)
            {
                _logger.LogError("Erro de requisição HTTP: {Message}", e);
            }
            catch (Exception e)
            {
                _logger.LogError("Ocorreu um erro inesperado: {Message}", e.Message);
            }

            return topMovies;
        }

        // Método para obter o diretor de cada filme usando o HTMLHelper
        public async Task<MovieInfo> ExtractMovieInfo(string movieUrl)
        {
            string data = string.Empty;
            UserAgentManager userAgentManager = new UserAgentManager();
            string _userAgent = userAgentManager.GetRandomUserAgent();
            string director = String.Empty;
            int releaseYear = 0;
            //for para retries por instabilidade no frontend
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    _logger.LogInformation("Buscando diretor para o filme {MovieUrl}", movieUrl);

                    using (var client = new HttpClient())
                    {
                        // Configurando os cookies no HttpClient
                        if (_cookies != null && _cookies.Any())
                        {
                            var cookieHeader = string.Join("; ", _cookies.Select(c => $"{c.Name}={c.Value}"));
                            client.DefaultRequestHeaders.Add("Cookie", cookieHeader);
                        }
                        client.DefaultRequestHeaders.UserAgent.ParseAdd(_userAgent);
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
                        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("pt-BR,en;q=0.9");
                        // Enviar a requisição HTTP para o link do filme
                        var response = await client.GetAsync(movieUrl);
                        if (response.IsSuccessStatusCode)
                        {
                            string pageContent = await response.Content.ReadAsStringAsync();

                            // Usa o HtmlAgilityPack para processar o HTML pois o json que o frontend consome volta no fim do html de resposta
                            JObject jsonDataDirector = HTMLHelper.ExtractJsonFromScript(pageContent, "application/ld+json");
                            JObject jsonDataYearRelease = HTMLHelper.ExtractJsonFromScript(pageContent, "application/json");
                            if (jsonDataDirector != null && jsonDataYearRelease != null)
                            {
                                var directorList = new List<string>();
                                if (jsonDataDirector["director"] is JArray directorArray)
                                {

                                    foreach (var directorObj in directorArray)
                                    {
                                        // Adiciona o nome do diretor, caso exista
                                        var directorName = directorObj["name"]?.ToString() ?? "";
                                        if (!string.IsNullOrEmpty(directorName))
                                        {
                                            directorList.Add(directorName);
                                        }
                                    }
                                }



                                // Junta os nomes dos diretores com vírgula
                                director = string.Join(",", directorList);
                                string datePublished = string.Empty;
                                var releaseYearNode = jsonDataYearRelease["props"]?["pageProps"]?["aboveTheFoldData"]?["releaseYear"]?["year"];
                                if (releaseYearNode != null)
                                {
                                    datePublished = releaseYearNode.ToString();
                                }
                                releaseYear = int.Parse(datePublished);
                                _logger.LogInformation("Diretor encontrado: {Director}", director);
                                return new MovieInfo
                                {
                                    ReleaseYear = releaseYear,
                                    Director = director
                                };
                            }
                            else
                            {
                                _logger.LogWarning("Diretor não encontrado para o filme {MovieUrl}", movieUrl);
                            }
                        }
                        else
                        {
                            _logger.LogError("Erro ao obter os dados do filme. Status Code: {StatusCode}", response.StatusCode);
                        }
                    }
                }
                catch (HttpRequestException e)
                {
                    _logger.LogError("Erro de requisição HTTP ao buscar diretor: {Message}", e.Message);
                }
                catch (Exception e)
                {
                    _logger.LogError("Ocorreu um erro inesperado ao buscar o diretor: {Message}", e.Message);
                }
            }


            return new MovieInfo
            {
                ReleaseYear = releaseYear,
                Director = director
            };
        }
        public async Task<List<CrawlerResult>> ProcessMoviesAsync(JObject jsonContent)
        {
            // Parseando o conteúdo JSON
            List<CrawlerResult> crawlerResults = new List<CrawlerResult>();
            JArray items = jsonContent["itemListElement"] as JArray ?? new JArray();

            if (jsonContent["itemListElement"] is JArray array)
            {
                items = array;
            }
            if (items == null)
            {
                _logger.LogWarning("A chave 'itemListElement' não foi encontrada ou não contém um array válido.");
                items = new JArray(); // Garantir que items seja um array vazio caso a chave seja nula ou inválida
            }

            var limitedItems = items.Take(20).ToList();
            Console.WriteLine($"Total Filmes {limitedItems.Count}");

            for (int i = 0; i < limitedItems.Count; i++)
            {
                // Extraindo os dados de cada filme
                var movieItem = items[i]["item"];

                // Verificar se movieItem não é nulo antes de acessar suas chaves
                if (movieItem != null)
                {
                    string alternateName = movieItem["alternateName"]?.ToString() ?? "";
                    double rating = movieItem["aggregateRating"]?["ratingValue"]?.ToObject<double>() ?? 0.0;
                    int ratingCount = movieItem["aggregateRating"]?["ratingCount"]?.ToObject<int>() ?? 0;
                    string url = movieItem["url"]?.ToString() ?? "";

                    // Evitar erro de null
                    if (string.IsNullOrEmpty(url))
                    {
                        _logger.LogWarning("URL não encontrada para o filme.");
                        continue; // Ignorar este item se a URL estiver ausente
                    }

                    var moveExtrasInfos = await ExtractMovieInfo(url);
                    string director = moveExtrasInfos.Director;
                    int YearPublished = moveExtrasInfos.ReleaseYear;

                    var crawlerResult = new CrawlerResult
                    {
                        Name = alternateName,
                        ReleaseYear = YearPublished,
                        Director = director,
                        AverageRating = rating,
                        NumberOfRatings = ratingCount
                    };

                    crawlerResults.Add(crawlerResult);
                }
                else
                {
                    _logger.LogError("O item do filme na posição {Index} é nulo.", i);
                }
            }
            return crawlerResults;
        }
    }
}