using OpenQA.Selenium;

namespace Equifax.Api.Helper
{
    public class BlockMatchingLoader
    {
        private readonly ElementLoader _elementLoader;
        private readonly BlocksLoader _blockLoader;
        private readonly SleepLoader _sleepLoader;

        public BlockMatchingLoader
            (
                ElementLoader elementLoader,
                BlocksLoader blocksLoader,
                SleepLoader sleepLoader
            )
        {
            _elementLoader = elementLoader;
            _blockLoader = blocksLoader;
            _sleepLoader = sleepLoader;
        }


        public async Task<IWebDriver> Matching(List<(IWebElement, int Index, string Type)> blockElementsWithIndex, string creditor_name, string open_date, IWebDriver driver)
        {
            IWebElement? matchedBlock = null;
            string viewDetailsButtonXPath = string.Empty;


            blockElementsWithIndex = blockElementsWithIndex.Distinct().ToList();

            for (int i = 0; i < blockElementsWithIndex.Count; i++)
            {
                var block = blockElementsWithIndex[i].Item1;
                int index = blockElementsWithIndex[i].Index;
                string type = blockElementsWithIndex[i].Type;

                try
                {
                    string installmentviewDetailsButtonXPath = string.Empty;
                    string revolvingviewDetailsButtonXPath = string.Empty;
                    string mortgageviewDetailsButtonXPath = string.Empty;
                    string otherviewDetailsButtonXPath = string.Empty;


                    // Find the Creditor Name
                    string creditorNameText = block.FindElement(By.XPath($".//*[@id=\"account-card-name\"]")).Text.Trim().ToLower();

                    // Compare the Creditor Name
                    if (creditorNameText.Equals(creditor_name.Trim().ToLower(), StringComparison.OrdinalIgnoreCase))
                    {
                        installmentviewDetailsButtonXPath = $"//*[@id=\"account-cards-list-installment-card-button-{index}\"]";
                        revolvingviewDetailsButtonXPath = $"//*[@id=\"account-cards-list-revolving-card-button-{index}\"]";
                        mortgageviewDetailsButtonXPath = $"//*[@id=\"account-cards-list-mortgage-card-button-{index}\"]";
                        otherviewDetailsButtonXPath = $"//*[@id=\"account-cards-list-other-card-button-{index}\"]";

                        if (type == "install")
                        {
                            viewDetailsButtonXPath = installmentviewDetailsButtonXPath;
                        }
                        else if (type == "revolving")
                        {
                            viewDetailsButtonXPath = revolvingviewDetailsButtonXPath;
                        }
                        else if (type == "mortgage")
                        {
                            viewDetailsButtonXPath = mortgageviewDetailsButtonXPath;
                        }

                        // Here type == other
                        else
                        {
                            viewDetailsButtonXPath = otherviewDetailsButtonXPath;
                        }


                        // Navigate to the next page to get the open-date Hidden field Value
                        _elementLoader.Load(viewDetailsButtonXPath, driver);

                        // Retrieve open date from next page
                        IWebElement? openDateElement = null;
                        string openDateText = string.Empty;
                        bool elementFound = false;

                        for (int attempt = 0; attempt < 3 && !elementFound; attempt++)
                        {
                            try
                            {
                                IJavaScriptExecutor jsExecutor = (IJavaScriptExecutor)driver;
                                openDateElement = driver.FindElement(By.XPath("//*[@id='credit-account-details-section-date-opened']"));
                                openDateText = (string)jsExecutor.ExecuteScript("return arguments[0].textContent;", openDateElement);

                                elementFound = true;
                            }
                            catch (StaleElementReferenceException)
                            {
                                Console.WriteLine($"Attempt {attempt + 1}: Stale Element Reference. Retrying...");
                            }
                        }

                        if (elementFound)
                        {
                            // Compare open dates
                            if (string.Equals(openDateText.Trim(), open_date.Trim(), StringComparison.InvariantCulture))
                            {
                                _sleepLoader.Seconds(3);
                                var backbtn = driver.FindElement(By.Id("account-back-button"));

                                backbtn.Click();
                                Console.WriteLine("-----BACK BUTTON Clicked.-----");

                                _sleepLoader.Seconds(5);
                                blockElementsWithIndex = await _blockLoader.Process(driver);

                                // Get the block again after coming back
                                block = blockElementsWithIndex[i].Item1;

                                matchedBlock = block;
                                break;
                            }
                            else
                            {
                                // Next Block Iteration.
                                continue;
                            }
                        }
                    }
                }
                catch (NoSuchElementException ex)
                {
                    Console.WriteLine($"Element not found in block {index}: {ex.Message}");
                }

                Console.WriteLine($"Block Index: {index}, Block Element: {block}");
            }


            // IF Block FOUND - Balle Balle 👌
            if (matchedBlock != null)
            {
                string FileADisputeButtonXPath = "//*[@id=\"credit-account-details-page-dispute-information-btn\"]";
                string continueXPath = "//*[@id=\"dispute-nav-buttons-continue-button\"]";

                // Click on the view details button in the matched block
                _elementLoader.Load(viewDetailsButtonXPath, driver);
                _elementLoader.Load(FileADisputeButtonXPath, driver);

                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                var radioButtons = driver.FindElements(By.Name("creditAccountIssue"));

                if (radioButtons.Count >= 2)
                {
                    js.ExecuteScript("arguments[0].click();", radioButtons[1]);
                }
                else
                {
                    Console.WriteLine("Issue with Radio Button Load.");
                }



                _elementLoader.Load(continueXPath, driver);
            }
            else
            {
                Console.WriteLine("No matching block found.");
            }

            return driver;
        }
    }
}
