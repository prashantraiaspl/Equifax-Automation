using OpenQA.Selenium;
using Equifax.Api.Domain.DTOs;
using Equifax.Api.Helper;

namespace Equifax.Api.Utilities
{
    public class BrowserUtility
    {
        private readonly DriverSetupManager _driverSetupManager;
        private readonly ElementLoader _elementLoader;
        private readonly BlocksLoader _blockLoader;
        private readonly MatchedBlock _matchedBlock;
        private readonly CheckboxLoader _checkboxLoader;

        public BrowserUtility
            (   DriverSetupManager driverSetupManager,
                ElementLoader elementLoader,
                BlocksLoader blocksLoader,
                MatchedBlock matchedBlock,
                CheckboxLoader checkboxLoader
            )
        {
            _driverSetupManager = driverSetupManager;
            _elementLoader = elementLoader;
            _blockLoader = blocksLoader;
            _matchedBlock = matchedBlock;
            _checkboxLoader = checkboxLoader;
        }



        public async Task<ResponseBody> BrowserAutomationProcess(string url, LoginCredentialRequestDto loginCredentials, DisputeRequestDto disputeRequest)
        {
            var browserResponse = new ResponseBody();

            try
            {
                IWebDriver? driver = null;

                driver = _driverSetupManager.InitializeDriver();

                System.Threading.Thread.Sleep(3000);

                // Step 1: Open URL
                OpenBrowserAndNavigate(url, driver);

                // Step 2: Perform Login
                Login(loginCredentials, driver);

                System.Threading.Thread.Sleep(5000);

                // Step 3: Navigate to Dispute
                NavigateToDispute(driver);

                System.Threading.Thread.Sleep(5000);

                // Step 4: File a Dispute
                await FileDisputeAsync(disputeRequest, driver);

                // Step 5: Close the Browser
                //CloseBrowser(driver);
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
                System.Threading.Thread.Sleep(3000);

                // Check if the subscription page is shown (span with price $19.95)
                try
                {
                    var declineButton = driver.FindElement(By.XPath("//*[@id=\"no\"]"));

                    if (declineButton != null)
                    {
                        declineButton.Click();

                        Console.WriteLine("Subscription prompt bypassed.");
                        System.Threading.Thread.Sleep(3000);

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

        public void NavigateToDispute(IWebDriver driver)
        {
            try
            {
                string disputeTabXPath = "//*[@id='fullDisputeLink']";

                _elementLoader.Load(disputeTabXPath, driver);

                Console.WriteLine("Navigated to dispute tab.");
                System.Threading.Thread.Sleep(3000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Navigation Error: {ex.Message}");
            }
        }

        public async Task FileDisputeAsync(DisputeRequestDto disputeRequest, IWebDriver driver)
        {
            List<(IWebElement Element, int Index, string Type)> blockElementsWithIndex = new List<(IWebElement Element, int Index, string Type)>();

            try
            {
                string creditor_name = string.Empty;
                string open_date = string.Empty;
                List<string> reasonArr = new List<string>();
                string comment = string.Empty;
                string confirmation_number = string.Empty;


                foreach (var account in disputeRequest.equifax_data.account)
                {
                    if (account.creditor_name != null)
                    {
                        creditor_name = account.creditor_name;
                    }
                    if (account.open_date != null)
                    {
                        open_date = account.open_date;
                    }
                    if (account.reason != null && account.reason.Count > 0)
                    {
                        reasonArr.AddRange(account.reason);
                    }
                    if (account.comment != null)
                    {
                        comment = account.comment;
                    }
                }

                string buttonXPath = "//*[@id=\"file-distupe-section-file-a-dispute-button\"]";
                string checkboxXPath = "//*[@id=\"onlineDeliveryAccept\"]/label/span[1]";
                string submitButtonXPath = "//*[@id=\"ssn-agree-modal-confirm-button\"]";
                string creditAccountXPath = "//*[@id=\"creditAccounts-section-link\"]/i";


                _elementLoader.Load(buttonXPath, driver);
                System.Threading.Thread.Sleep(3000);

                var element = driver.FindElement(By.XPath(checkboxXPath));
                element.Click();
                System.Threading.Thread.Sleep(3000);

                _elementLoader.Load(submitButtonXPath, driver);
                System.Threading.Thread.Sleep(5000);

                _elementLoader.Load(creditAccountXPath, driver);
                System.Threading.Thread.Sleep(3000);


                // Block Loader
                blockElementsWithIndex = await _blockLoader.Process(driver);


                IWebDriver driver_matchedBlock = await _matchedBlock.Matching(blockElementsWithIndex, creditor_name, open_date, driver);

                var checkBoxArr = new List<string>
                {
                    "//*[@id=\"dispute-checkbox-group-field-007\"]", // For 'The status, payment history, or payment rating...'
                    "//*[@id=\"dispute-checkbox-group-field-012\"]", // For 'I paid this account before it was closed...'
                    "//*[@id=\"dispute-checkbox-group-field-013\"]", // For 'The balance or past due amount is not correct.'
                    "//*[@id=\"dispute-checkbox-group-field-015\"]", // For 'The credit limit or high credit amount is inaccurate.'
                    "//*[@id=\"dispute-checkbox-group-field-016\"]", // For 'The date of first delinquency is inaccurate.'
                    "//*[@id=\"dispute-checkbox-group-field-024\"]", // For 'I closed this account.'
                    "//*[@id=\"dispute-checkbox-group-field-028\"]", // For 'The comment from the lender/creditor is not correct.'
                    "//*[@id=\"dispute-checkbox-group-field-037\"]"  // For 'This account is included in my bankruptcy.'
                };


                foreach (var checkBoxXPath in checkBoxArr)
                {
                    _checkboxLoader.CheckboxHandelling(checkBoxXPath, driver_matchedBlock, reasonArr);
                }

                string commentXPath = "//*[@id=\"creditAccountInfoComment\"]";
                string saveBtnXPath = "//*[@id=\"dispute-comment-save-button-2\"]";
                string continueBtnXPath = "//*[@id=\"dispute-nav-buttons-continue-button\"]";
                string skipUploadXPath = "//*[@id=\"dispute-nav-buttons-skip-button\"]";
                string submitDisputeXPath = "//*[@id=\"dispute-review-finish-and-upload-button\"]";
                string confirmationNumberXPath = "//*[@id=\"dispute-confirmation-cards-list-confirmation-number\"]";

                System.Threading.Thread.Sleep(3000);
                IWebElement commentElement = driver.FindElement(By.XPath(commentXPath));
                commentElement.Clear();
                commentElement.SendKeys(comment);

                _elementLoader.Load(saveBtnXPath, driver);
                _elementLoader.Load(continueBtnXPath, driver);
                _elementLoader.Load(skipUploadXPath, driver);
                //_elementLoader.Load(submitDisputeXPath, driver);

                System.Threading.Thread.Sleep(5000);
                string confirmation_number_text = driver.FindElement(By.XPath(confirmationNumberXPath)).Text;

                // Storing Confirmation Number
                System.Threading.Thread.Sleep(3000);
                confirmation_number = confirmation_number_text;
                Console.WriteLine($"confirmation_number: {confirmation_number}");
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
