using Microsoft.Extensions.Logging;
using IMDB_Crawler.Crawler.Core.Utils;
using IMDB_Crawler.Crawler.Core.Models;

using IMDB_Crawler.Crawler.Core;
using Microsoft.Extensions.Logging.Console;
using NLog;

class Program
{
    static async Task Main()
    {
        string loginUrl = "https://www.imdb.com/registration/signin/?ref=nv_generic_lgin&u=%2Fpt%2F"; // Substitua pelo URL real

        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            // Adiciona o ConsoleLogger e personaliza o formato
            builder.AddConsole(options =>
            {
                options.TimestampFormat = "yyyy-MM-dd HH:mm:ss "; // Define o formato do timestamp
            });
        });

        ILogger<LoginRPA> loggerLogin = loggerFactory.CreateLogger<LoginRPA>();
        ILogger<IMDBCrawler> loggerCrawler = loggerFactory.CreateLogger<IMDBCrawler>();


        //fazendo login via RPA para obter os cookies para o Crawler
        List<OpenQA.Selenium.Cookie> cookies = await LoginAttempts(loginUrl, loggerLogin);

        if (cookies.Count == 0) {
            Console.WriteLine("Não obtivemos retorno de cookies, tentando novamente");
            cookies = await LoginAttempts(loginUrl, loggerLogin);
        }
        // Faz a extração dos dados
        IMDBCrawler extractData = new IMDBCrawler(cookies, loggerCrawler);
        List<CrawlerResult> extratedData = await extractData.ExtractTop20MoviesAsync();
        foreach (var movie in extratedData)
        {
            Console.WriteLine($"Nome: {movie.Name}");
            Console.WriteLine($"Ano de Lançamento: {movie.ReleaseYear}");
            Console.WriteLine($"Diretor: {movie.Director}");
            Console.WriteLine($"Avaliação Média: {movie.AverageRating}");
            Console.WriteLine($"Número de Avaliações: {movie.NumberOfRatings}");
            Console.WriteLine(new string('-', 50)); // Separador
        }

        // Criar instância do CsvExporter
        var csvExporter = new CsvExporter();

        // Exportar os dados para CSV
        csvExporter.SaveToCsv(extratedData);

    }

    static async Task<List<OpenQA.Selenium.Cookie>> LoginAttempts(string loginUrl, ILogger<LoginRPA> logger)
    {
        string username = string.Empty;
        string password = string.Empty;
        Console.WriteLine("Digite as credênciais de usa conta no IMDB ");
        while (string.IsNullOrEmpty(username))
        {
            Console.Write("Digite seu email: ");
            username = Console.ReadLine();
            if (string.IsNullOrEmpty(username))
            {
                Console.WriteLine("O email é obrigatório. Por favor, insira o email.");
            }
        }

        while (string.IsNullOrEmpty(password))
        {
            Console.Write("Digite sua senha: ");
            password = Console.ReadLine();
            if (string.IsNullOrEmpty(password))
            {
                Console.WriteLine("A senha é obrigatória. Por favor, insira a senha.");
            }
        }

        // Cria uma instância do LoginRPA com as credenciais fornecidas
        var loginRPA = new LoginRPA(loginUrl, username, password, logger);


        // Realiza o login e captura os cookies
        List<OpenQA.Selenium.Cookie> cookies = await loginRPA.Login();

        return cookies;
    }
}