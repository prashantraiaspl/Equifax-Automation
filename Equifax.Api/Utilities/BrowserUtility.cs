using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using Equifax.Api.Domain.DTOs;
using WebDriverManager.DriverConfigs.Impl;
using WebDriverManager.Helpers;
using WebDriverManager;
using OpenQA.Selenium.Support.UI;

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
            string extensionPath = @"D:\Prashant\Equifax API\Equifax.Api\CRX File\GCKNHKKOOLAABFMLNJONOGAAIFNJLFNP_8_9_0_0.crx";

            if (!System.IO.File.Exists(extensionPath))
            {
                throw new System.IO.FileNotFoundException("No extension found at the specified path", extensionPath);
            }

            // Initialize Chrome options
            var chromeOptions = new ChromeOptions();

            // Add the FoxyProxy extension to Chrome
            chromeOptions.AddExtensions(extensionPath);

            // Proxy settings
            string proxyUrl = "162.219.27.154";
            string proxyPort = "6025";
            string proxyUsername = "e8r5x";
            string proxyPassword = "8svwlybs";

            var proxy = new Proxy
            {
                HttpProxy = $"{proxyUsername}:{proxyPassword}@{proxyUrl}:{proxyPort}",
                SslProxy = $"{proxyUsername}:{proxyPassword}@{proxyUrl}:{proxyPort}",
            };

            // Assign proxy to ChromeOptions
            chromeOptions.Proxy = proxy;

            IWebDriver driver = new ChromeDriver(chromeOptions);
            driver.Manage().Window.Maximize();

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(1));

            // Navigate to the Chrome extension settings page to set the proxy via UI
            driver.Navigate().GoToUrl("chrome-extension://gcknhkkoolaabfmlnjonogaaifnjlfnp/content/options.html");


            try
            {
                // Wait for the "Proxies" label to be visible and click it
                var proxiesLabel = wait.Until(d => d.FindElement(By.XPath("//label[@for='nav4']")));
                proxiesLabel.Click();

                // Wait for the "Add" button to be visible and click it
                var addButton = wait.Until(d => d.FindElement(By.XPath("//button[contains(text(),'Add')]")));
                addButton.Click();

                // Now fill in the proxy details
                driver.FindElement(By.XPath("//input[@data-id='hostname']")).SendKeys("162.219.27.154");
                driver.FindElement(By.XPath("//input[@data-id='port']")).SendKeys("6025");
                driver.FindElement(By.XPath("//input[@data-id='username']")).SendKeys("e8r5x");
                driver.FindElement(By.XPath("//input[@data-id='password']")).SendKeys("8svwlybs");

                // Submit Button
                var saveButton = driver.FindElement(By.XPath("/html/body/article/section[4]/fieldset/button"));
                saveButton.Click();


                // After clicking save, open a new tab
                driver.FindElement(By.TagName("body")).SendKeys(Keys.Control + "t");
                driver.Navigate().GoToUrl("chrome-extension://gcknhkkoolaabfmlnjonogaaifnjlfnp/content/popup.html");

                Console.WriteLine(driver.Title);

                // Wait for the specific link in the popup and click it
                var popupLink = wait.Until(d => d.FindElement(By.XPath("/html/body/article/section/div[1]/label[3]/span[2]")));
                popupLink.Click();

                // Switch back to the original tab if needed (optional)
                List<string> tabs = new List<string>(driver.WindowHandles);
                driver.SwitchTo().Window(tabs[0]);

            }
            catch (NoSuchElementException ex)
            {
                Console.WriteLine($"Element not found: {ex.Message}");
            }
            catch (WebDriverTimeoutException ex)
            {
                Console.WriteLine($"Timeout waiting for element: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }



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
                System.Threading.Thread.Sleep(3000);
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
                System.Threading.Thread.Sleep(3000);
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
