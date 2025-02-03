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

        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole(options =>
            {
                options.TimestampFormat = "yyyy-MM-dd HH:mm:ss "; // Define o formato do timestamp
            });
        });

        ILogger<LoginRPA> loggerLogin = loggerFactory.CreateLogger<LoginRPA>();
        ILogger<IMDBCrawler> loggerCrawler = loggerFactory.CreateLogger<IMDBCrawler>();


        //fazendo login via RPA para obter os cookies para o Crawler
        List<OpenQA.Selenium.Cookie> cookies = LoginAttempts(loginUrl, loggerLogin);

        if (cookies.Count == 0) {
            Console.WriteLine("Não obtivemos retorno de cookies, tentando novamente");
            cookies = LoginAttempts(loginUrl, loggerLogin);
        }
        // Faz a extração dos dados
        IMDBCrawler extractData = new IMDBCrawler(cookies, loggerCrawler);
        List<CrawlerResult> extratedData = await extractData.ExtractTopMovies();
        foreach (var movie in extratedData)
        {
            Console.WriteLine($"Nome: {movie.Name}");
            Console.WriteLine($"Ano de Lançamento: {movie.ReleaseYear}");
            Console.WriteLine($"Diretor: {movie.Director}");
            Console.WriteLine($"Avaliação Média: {movie.AverageRating}");
            Console.WriteLine($"Número de Avaliações: {movie.NumberOfRatings}");
            Console.WriteLine(new string('-', 50));
        }

        // Criar instância do CsvExporter
        var csvExporter = new CsvExporter();

        // Exportar os dados para CSV
        csvExporter.SaveToCsv(extratedData);

    }

    static List<OpenQA.Selenium.Cookie> LoginAttempts(string loginUrl, ILogger<LoginRPA> logger)
    {
        AppConfig _config = AppConfig.Load();

        string username = _config.EmailLogin;
        string password = _config.PasswordLogin;
        

        // Cria uma instância do LoginRPA com as credenciais fornecidas
        var loginRPA = new LoginRPA(loginUrl, username, password, logger);


        // Realiza o login e captura os cookies
        List<OpenQA.Selenium.Cookie> cookies = loginRPA.Login();

        return cookies;
    }
}