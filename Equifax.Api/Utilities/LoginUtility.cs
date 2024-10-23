using Equifax.Api.Domain.DTOs;
using Equifax.Api.Helper;
using OpenQA.Selenium;

namespace Equifax.Api.Utilities
{
    public class LoginUtility
    {
        private readonly SleepLoader _sleepLoader;

        public LoginUtility(SleepLoader sleepLoader)
        {
            _sleepLoader = sleepLoader;
        }


        public void Login(LoginCredentialRequestDto request, IWebDriver driver)
        {
            try
            {
                // Now you can find the elements
                var emailInput = driver.FindElement(By.Id("login-email"));
                var passwordInput = driver.FindElement(By.Id("login-password"));
                var submitButton = driver.FindElement(By.Id("login-button"));

                // Enter email and password
                emailInput.SendKeys(request.user_name);
                passwordInput.SendKeys(request.user_password);
                submitButton.Click();


                Console.WriteLine("Login successful.");
                _sleepLoader.Seconds(3);

                // Check if the subscription page is shown (span with price $19.95)
                try
                {
                    var declineButton = driver.FindElement(By.XPath("//*[@id=\"no\"]"));

                    if (declineButton != null)
                    {
                        declineButton.Click();

                        Console.WriteLine("Subscription prompt bypassed.");
                        _sleepLoader.Seconds(3);

                        // After clicking, navigate to the dashboard
                        driver.Navigate().GoToUrl("https://my.equifax.com/membercenter/#/dashboard");
                        Console.WriteLine("Navigated to dashboard.");
                    }
                    else
                    {
                        Console.WriteLine("No subscription prompt detected.");
                    }
                }
                catch (NoSuchElementException)
                {
                    // The subscription span was not found, meaning no subscription prompt was shown
                    Console.WriteLine("No subscription page detected, continuing...");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login Error: {ex.Message}");
            }
        }
    }
}
