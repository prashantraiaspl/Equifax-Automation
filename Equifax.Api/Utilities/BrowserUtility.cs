using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using Equifax.Api.Domain.DTOs;

namespace Equifax.Api.Utilities
{
    public static class BrowserUtility
    {
        public static void OpenBrowserAndNavigate(string url, LoginCredentialRequestDto requestData)
        {
            // Initialize the ChromeDriver
            using (IWebDriver driver = new ChromeDriver())
            {
                try
                {
                    driver.Navigate().GoToUrl(url);
                    Console.WriteLine($"Navigated to: {url}");

                    System.Threading.Thread.Sleep(5000);
                    //WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(3));

                    driver.Manage().Window.Maximize();


                    // Now you can find the elements
                    var emailInput = driver.FindElement(By.Id("login-email"));
                    var passwordInput = driver.FindElement(By.Id("login-password"));
                    var submitButton = driver.FindElement(By.Id("login-button"));

                    // Enter email and password
                    emailInput.SendKeys(requestData.Email);
                    passwordInput.SendKeys(requestData.Password);
                    submitButton.Click();

                    Console.WriteLine("Press any key to close the browser...");
                    Console.ReadKey();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
                finally
                {
                    driver.Quit();
                }
            }
        }
    }

}
