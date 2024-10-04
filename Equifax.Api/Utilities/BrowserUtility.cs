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

        public BrowserUtility(DriverSetupManager driverSetupManager, ElementLoader elementLoader ,BlocksLoader blocksLoader)
        {
            _driverSetupManager = driverSetupManager;
            _elementLoader = elementLoader;
            _blockLoader = blocksLoader;
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
            List<(IWebElement Element, int Index)> blockElementsWithIndex = new List<(IWebElement Element, int Index)>();

            try
            {
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


                IWebElement? matchedBlock = null;
                string viewDetailsButtonXPath = string.Empty;

                blockElementsWithIndex = blockElementsWithIndex.Distinct().ToList();

                foreach (var (block, index) in blockElementsWithIndex)
                {
                    try
                    {
                        // Find the Creditor Name
                        var creditorNameElement = block.FindElement(By.XPath("//*[@id=\"account-card-name\"]"));
                        string creditorNameText = creditorNameElement.GetAttribute("innerText");

                        // Compare creditor name and open date
                        if (creditorNameText.Equals(disputeRequest.equifax_data.account[0].creditor_name, StringComparison.OrdinalIgnoreCase))
                        {
                            viewDetailsButtonXPath = $"//*[@id=\"account-cards-list-installment-card-button-{index}\"]";
                            System.Threading.Thread.Sleep(3000);
                            _elementLoader.Load(viewDetailsButtonXPath, driver);

                            IWebElement openDateElement = null;
                            bool elementFound = false;

                            for (int attempt = 0; attempt < 3 && !elementFound; attempt++)
                            {
                                try
                                {
                                    openDateElement = FindOpenDateElement(block);
                                    elementFound = true;
                                }
                                catch (StaleElementReferenceException)
                                {
                                    Console.WriteLine($"Attempt {attempt + 1}: Stale Element Reference. Retrying...");
                                }
                            }

                            // If element is found, retrieve the text
                            if (elementFound)
                            {
                                // Use JavaScriptExecutor to get the hidden element's content
                                IJavaScriptExecutor jsExecutor = (IJavaScriptExecutor)driver;
                                string openDateText = (string)jsExecutor.ExecuteScript("return arguments[0].textContent;", openDateElement);

                                // Compare open dates
                                if (openDateText == disputeRequest.equifax_data.account[0].open_date)
                                {
                                    matchedBlock = block;
                                    break; // Exit loop once a match is found
                                }
                            }
                        }

                        // Generate view details button XPath for the matched block index
                        viewDetailsButtonXPath = $"//*[@id=\"account-cards-list-installment-card-button-{index}\"]";
                    }
                    catch (NoSuchElementException ex)
                    {
                        Console.WriteLine($"Element not found in block {index}: {ex.Message}");
                    }

                    Console.WriteLine($"Block Index: {index}, Block Element: {block}");
                }


                if (matchedBlock != null)
                {
                    // Click on the view details button in the matched block
                    string FileADisputeButtonXPath = "//*[@id=\"credit-account-details-page-dispute-information-btn\"]";
                    string disputeIssueOptionXPath = "//*[@id=\"dispute-radio-group-field\"]/div/efx-radio-group-field/fieldset/div[1]/div/div[2]/efx-radio-button/label";
                    string continueXPath = "//*[@id=\"dispute-nav-buttons-continue-button\"]";

                    System.Threading.Thread.Sleep(5000);
                    _elementLoader.Load(viewDetailsButtonXPath, driver);
                    _elementLoader.Load(FileADisputeButtonXPath, driver);
                    _elementLoader.Load(disputeIssueOptionXPath, driver);
                    _elementLoader.Load(continueXPath, driver);
                }
                else
                {
                    Console.WriteLine("No matching block found.");
                }
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

        private IWebElement FindOpenDateElement(IWebElement block)
        {
            return block.FindElement(By.XPath("//*[@id=\"credit-account-details-section-date-opened\"]"));
        }

    }

}
