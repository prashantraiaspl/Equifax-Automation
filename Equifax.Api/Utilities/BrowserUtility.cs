using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using Equifax.Api.Domain.DTOs;

namespace Equifax.Api.Utilities
{
    public class BrowserUtility
    {
        private readonly IConfiguration _configuration;

        public BrowserUtility(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public IWebDriver InitializeDriver()
        {
            var proxyUrl = _configuration.GetValue<string>("ProxySettings:ProxyUrl");
            var proxyPort = _configuration.GetValue<int>("ProxySettings:Port");
            var proxyUsername = _configuration.GetValue<string>("ProxySettings:Username");
            var proxyPassword = _configuration.GetValue<string>("ProxySettings:Password");

            var proxy = new Proxy
            {
                HttpProxy = $"{proxyUsername}:{proxyPassword}@{proxyUrl}:{proxyPort}",
                SslProxy = $"{proxyUsername}:{proxyPassword}@{proxyUrl}:{proxyPort}"
            };


            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("--start-maximized");
            chromeOptions.Proxy = proxy;

            IWebDriver driver = new ChromeDriver(chromeOptions);
            driver.Manage().Window.Maximize();
            return driver;
        }


        public ResponseBody BrowserAutomationProcess(string url, LoginCredentialRequestDto loginCredentials)
        {
            var browserResponse = new ResponseBody();

            IWebDriver? driver = null;
            try
            {
                driver = InitializeDriver();

                // Step 1: Open URL
                OpenBrowserAndNavigate(url, driver);

                // Step 2: Perform Login
                Login(driver, loginCredentials);

                // Step 3: Navigate to Dispute
                NavigateToDispute(driver);

                // Step 4: File a Dispute
                FileDispute(driver);

                CloseBrowser(driver);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during browser interaction: {ex.Message}");
            }
            return browserResponse;
        }

        public void OpenBrowserAndNavigate(string url, IWebDriver driver)
        {
            try
            {
                driver.Navigate().GoToUrl(url);
                System.Threading.Thread.Sleep(5000);
                Console.WriteLine($"Navigated to: {url}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public void Login(IWebDriver driver, LoginCredentialRequestDto requestData)
        {
            try
            {
                // Now you can find the elements
                var emailInput = driver.FindElement(By.Id("login-email"));
                var passwordInput = driver.FindElement(By.Id("login-password"));
                var submitButton = driver.FindElement(By.Id("login-button"));

                // Enter email and password
                emailInput.SendKeys(requestData.Email);
                passwordInput.SendKeys(requestData.Password);
                submitButton.Click();

                Console.WriteLine("Login successful.");
                System.Threading.Thread.Sleep(5000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login Error: {ex.Message}");
            }
        }

        public void NavigateToDispute(IWebDriver driver)
        {
            try
            {
                var disputeTab = driver.FindElement(By.Id("fullDisputeLink"));
                disputeTab.Click();
                Console.WriteLine("Navigated to dispute tab.");
                System.Threading.Thread.Sleep(2000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Navigation Error: {ex.Message}");
            }
        }

        public void FileDispute(IWebDriver driver)
        {
            try
            {
                var disputeButton = driver.FindElement(By.Id("file-distupe-section-file-a-dispute-button"));
                disputeButton.Click();
                System.Threading.Thread.Sleep(2000);

                var checkbox = driver.FindElement(By.Id("onlineDeliveryAccept"));
                if (!checkbox.Selected)
                {
                    checkbox.Click();
                }

                var submitButton = driver.FindElement(By.Id("ssn-agree-modal-confirm-button"));
                submitButton.Click();
                System.Threading.Thread.Sleep(2000);

                Console.WriteLine("Dispute filed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Filing Dispute Error: {ex.Message}");
            }
        }

        public void CloseBrowser(IWebDriver driver)
        {
            driver.Quit();
        }
    }

}
