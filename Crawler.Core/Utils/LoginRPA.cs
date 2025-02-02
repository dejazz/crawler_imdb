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

        public async Task<List<Cookie>> Login()
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

                ValidateCaptcha(driver);

                bool isNavegated = NavigateToRatings(driver);
                if (!isNavegated)
                {
                    NavigateToRatings(driver);
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
            var errorLoginClass = driver.FindElements(By.ClassName("auth-error-message-box"));
            if (errorLoginClass.Count > 0)
            {
                _logger.LogError("Usuário ou senha errados, favor tentar novamente.");
                return true;
            }
            return false;
        }

        private void ValidateCaptcha(IWebDriver driver)
        {
            _logger.LogInformation("Verificando Captcha...");
            TimeSpan timeout = TimeSpan.FromMinutes(1);
            DateTime start = DateTime.Now;
            while ((DateTime.Now - start) < timeout)
            {
                if (!driver.FindElements(By.Name("cvf_captcha_captcha_token")).Any())
                {
                    _logger.LogInformation("Captcha não localizado, continuando a automação...");
                
                    break;
                }
                _logger.LogInformation("Captcha localizado, por favor o resolva :)");
                Thread.Sleep(2000);
            }
        }

        private bool NavigateToRatings(IWebDriver driver)
        {
            try
            {
                IWebElement listFavoriteButton = driver.FindElement(By.LinkText("Lista de favoritos"));
                _logger.LogInformation("Achou lista de favoritos");
                listFavoriteButton.Click();
                _logger.LogInformation("Clicou na lista de favoritos");
                Thread.Sleep(5000);
                var elementMyClassificationButton = new WebDriverWait(driver, TimeSpan.FromSeconds(30))
                    .Until(d => d.FindElement(By.XPath("//a[contains(@href, 'ratings')]")));

                string hrefValue = elementMyClassificationButton.GetAttribute("href");
                _logger.LogInformation($"Valor do href para classificações: {hrefValue}");
                driver.Navigate().GoToUrl(hrefValue);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{ex}Erro ao encontrar pagina de classificação");
                return false;

            }

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
