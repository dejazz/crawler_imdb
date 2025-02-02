using Crawler_Data_Lawer.Crawler.Core.Utils;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IMDB_Crawler.Crawler.Core.Utils
{
    public class LoginRPA
    {
        private readonly string _loginUrl;
        private readonly string _username;
        private readonly string _password;
        private readonly ILogger<LoginRPA> _logger;

        public LoginRPA(string loginUrl, string username, string password, ILogger<LoginRPA> logger)
        {
            _loginUrl = loginUrl;
            _username = username;
            _password = password;
            _logger = logger;
        }

        public List<Cookie> Login()
        {
            _logger.LogInformation("Iniciando o processo de login...");
            using var driver = InitializeDriver();

            try
            {
                driver.Navigate().GoToUrl(_loginUrl);

                PerformLogin(driver);

                if (CheckLoginError(driver))
                {
                    return [];
                }

               bool isNavegated = NavigateToRatings(driver);
               if(!isNavegated)
                {
                    return [];
                }
                return CaptureCookies(driver);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro durante o processo de login: {ex.Message}");
                throw;
            }
            finally
            {
                driver.Close();
            }
        }

        private IWebDriver InitializeDriver()
        {
            var options = new ChromeOptions();
            var driver = new ChromeDriver(options);
            driver.Manage().Window.Size = new System.Drawing.Size(1380, 1080);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            return driver;
        }

        private void PerformLogin(IWebDriver driver)
        {
            _logger.LogInformation("Acessando a página de login...");
            driver.FindElement(By.Id("signin-options"));
            driver.FindElement(By.LinkText("Sign in with IMDb")).Click();
            _logger.LogInformation("Clicou no login com a conta do IMDB");

            driver.FindElement(By.Id("ap_email")).SendKeys(_username);
            _logger.LogInformation("Digitou email");

            driver.FindElement(By.Id("ap_password")).SendKeys(_password);
            _logger.LogInformation("Digitou senha");

            driver.FindElement(By.Id("signInSubmit")).Click();
            _logger.LogInformation("Clicou em Logar");
        }

        private bool CheckLoginError(IWebDriver driver)
        {
            _logger.LogInformation("Verificando se realmente logou.");

            var errorLoginClass = driver.FindElements(By.Id("auth-error-message-box"));

            //verifica se existe atributos html dos captchas de texto e imagem da amazon para o caso de erro de credenciais
            var errorLoginClassCaptcha = driver.FindElements(By.XPath("//img[@alt='captcha']"));
            var buttonPuzzle = driver.FindElements(By.XPath("//button[contains(text(), 'Start Puzzle')]"));

            //verifica se não caiu na página de captcha da amazon para o caso de erro de credenciais
            string expectedUrlPart = "https://www.imdb.com/ap/cvf/request?arb";
            string currentUrl = driver.Url;
            if (currentUrl.Contains(expectedUrlPart)) { 
                _logger.LogError("Usuário ou senha errados, favor tentar novamente.");
                return true;
            }
            
            if (errorLoginClass.Count > 0 || errorLoginClassCaptcha.Count > 0 || buttonPuzzle.Count > 0)
            {
                _logger.LogError("Usuário ou senha errados, favor tentar novamente.");
                return true;
            }
            return false;
        }

        static async Task<string> SolveCaptcha(IWebDriver driver) {

            //deve-se por a api key do anticaptcha
            string apiKey = "SUA_API_KEY_ANTICAPTCHA";
            var captchaImage = driver.FindElement(By.XPath("//img[@alt='captcha']"));

            string captchaUrl = captchaImage.GetAttribute("src");
            string imageUrl = captchaUrl; // Link da imagem do captcha
            Console.WriteLine($"imageUrl = {imageUrl}");
            var solver = new AntiCaptchaSolver(apiKey);
            string? result = await solver.SolveCaptchaFromUrlAsync(imageUrl);
            Console.WriteLine(result != null ? $"Captcha resolvido: {result}" : "Falha ao resolver captcha.");
            return result ?? "Captcha não resolvido";
        }
        private async void ValidateCaptcha(IWebDriver driver)
        {
            _logger.LogInformation("Verificando Captcha...");           
            if (!driver.FindElements(By.Name("cvf_captcha_captcha_token")).Any())
            {
                _logger.LogInformation("Captcha não localizado, continuando a automação...");
                return;
            }
            //chamando solução do captcha
            await SolveCaptcha(driver);


        }

        private bool NavigateToRatings(IWebDriver driver)
        {
            //loop pára retries em caso de erro
            for(int i = 0; i < 10; i++)
            {
                try
                {
                    _logger.LogInformation($"Tentativa {i+1}/10 de achar a lista de favoritos e classificações");
                    
                    //faz o goto para url de lista de usuario no next. Nesse caso o next internamente lê o ref e faz o direcionamento correto de acordo com as informações do usuário após logar
                    driver.Navigate().GoToUrl("https://www.imdb.com/list/watchlist/?ref_=nv_usr_wl_all_0");
                    Thread.Sleep(5000);

                    var elementMyClassificationButton = new WebDriverWait(driver, TimeSpan.FromSeconds(30))
                        .Until(d => d.FindElement(By.XPath("//a[contains(@href, 'ratings')]")));

                    string hrefValue = elementMyClassificationButton.GetAttribute("href");
                    _logger.LogInformation($"Valor do href para classificações: {hrefValue}");
                    driver.Navigate().GoToUrl(hrefValue);
                    
                    //sleep para comprovar rpa na tela de classificação
                    Thread.Sleep(10000);
                    _logger.LogInformation($"Entrou na página de classificações");
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"{ex} \n Erro ao encontrar pagina de classificação. Tentando novamente");

                }

            }
            return false;

        }

        private List<Cookie> CaptureCookies(IWebDriver driver)
        {
            _logger.LogInformation("Capturando cookies...");
            var cookies = driver.Manage().Cookies.AllCookies.ToList();
            foreach (var cookie in cookies)
            {
                _logger.LogInformation($"{cookie.Name} = {cookie.Value}");
            }
            return cookies;
        }
    }
}
