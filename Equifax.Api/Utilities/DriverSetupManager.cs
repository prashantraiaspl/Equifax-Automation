using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;

namespace Equifax.Api.Utilities
{
    public class DriverSetupManager
    {
        private readonly IConfiguration _configuration;

        public DriverSetupManager(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IWebDriver InitializeDriver()
        {
            // Path for .CRX File needed for Proxy Setup
            string extensionPath = _configuration["extensionPath"] ?? @"D:\Prashant\Equifax API\Equifax.Api\CRX File\GCKNHKKOOLAABFMLNJONOGAAIFNJLFNP_8_9_0_0.crx";

            // Initialize Chrome options
            var chromeOptions = new ChromeOptions();

            // Add the FoxyProxy extension to Chrome
            chromeOptions.AddExtensions(extensionPath);

            // Proxy settings
            var proxyUrl = _configuration.GetValue<string>("ProxySettings:ProxyUrl");
            var proxyPort = _configuration.GetValue<int>("ProxySettings:Port");
            var proxyUsername = _configuration.GetValue<string>("ProxySettings:Username");
            var proxyPassword = _configuration.GetValue<string>("ProxySettings:Password");

            var proxy = new Proxy
            {
                HttpProxy = $"{proxyUsername}:{proxyPassword}@{proxyUrl}:{proxyPort}",
                SslProxy = $"{proxyUsername}:{proxyPassword}@{proxyUrl}:{proxyPort}",
            };

            // Assign proxy to ChromeOptions
            chromeOptions.Proxy = proxy;

            IWebDriver driver = new ChromeDriver(chromeOptions);
            driver.Manage().Window.Maximize();

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(3));

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


                ((IJavaScriptExecutor)driver).ExecuteScript("window.open();");
                var tabs = new List<string>(driver.WindowHandles);

                // Switch to the new tab (last opened)
                driver.SwitchTo().Window(tabs[1]);

                driver.Navigate().GoToUrl("chrome-extension://gcknhkkoolaabfmlnjonogaaifnjlfnp/content/popup.html");

                Console.WriteLine(driver.Title);

                // Wait for the specific link in the popup and click it
                var popupLink = wait.Until(d => d.FindElement(By.XPath("/html/body/article/section/div[1]/label[3]/span[2]")));
                popupLink.Click();

                // Close the popup tab and return to the original tab
                driver.Close();
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

    }
}
