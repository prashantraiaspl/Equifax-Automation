using OpenQA.Selenium;
using System.Text.RegularExpressions;

namespace Equifax.Api.Helper
{
    public class BlocksLoader
    {
        public async Task<List<(IWebElement, int Index, string Type)>> Process(IWebDriver driver)
        {
            List<(IWebElement Element, int Index, string Type)> blockElementsWithIndex = new List<(IWebElement Element, int Index, string Type)>();

            try
            {
                // Extract the number of Total Open Accounts blocks
                int totalBlocks = ExtractBlockCount(driver, By.Id("credit-accounts-page-filter-option-openAccounts"), "Total Open Accounts");
                int revolvingBlocks = ExtractBlockCount(driver, By.XPath("//*[@id=\"account-cards-list-revolving-title\"]"), "Revolving Accounts");
                int installmentBlocks = ExtractBlockCount(driver, By.XPath("//*[@id=\"account-cards-list-installment-title\"]"), "Installment Accounts");
                int mortgageBlocks = ExtractBlockCount(driver, By.XPath("//*[@id=\"account-cards-list-mortgage-title\"]"), "Mortgage Accounts");
                int otherBlocks = ExtractBlockCount(driver, By.XPath("//*[@id=\"account-cards-list-other-title\"]"), "Other Accounts");

                bool isRevolvingChecked = false;
                bool isInstallmentChecked = false;
                bool isMortgageChecked = false;
                bool isOtherChecked = false;

                // Process each block dynamically
                for (int j = 0; j < totalBlocks; j++)
                {
                    if (revolvingBlocks > 0 && !isRevolvingChecked)
                    {
                        for (int i = 0; i < revolvingBlocks; i++)
                        {
                            string dynamicId = $"account-cards-list-revolving-card-{i}";

                            try
                            {
                                var blockElement = driver.FindElement(By.Id(dynamicId));
                                if (blockElement != null)
                                {
                                    blockElementsWithIndex.Add((blockElement, i, "revolving"));
                                    Console.WriteLine($"Added Revolving Block {i}");
                                }
                            }
                            catch (NoSuchElementException ex)
                            {
                                Console.WriteLine($"No element found with ID {dynamicId}: {ex.Message}");
                            }
                        }
                        isRevolvingChecked = true;
                    }

                    else if (installmentBlocks > 0 && !isInstallmentChecked)
                    {
                        for (int i = 0; i < installmentBlocks; i++)
                        {
                            string dynamicId = $"account-cards-list-installment-card-{i}";

                            try
                            {
                                var blockElement = driver.FindElement(By.Id(dynamicId));
                                if (blockElement != null)
                                {
                                    blockElementsWithIndex.Add((blockElement, i, "install"));
                                    Console.WriteLine($"Added Installment Block {i}");
                                }
                            }
                            catch (NoSuchElementException ex)
                            {
                                Console.WriteLine($"No element found with ID {dynamicId}: {ex.Message}");
                            }
                        }
                        isInstallmentChecked = true;
                    }

                    else if (mortgageBlocks > 0 && !isMortgageChecked)
                    {
                        for (int i = 0; i < mortgageBlocks; i++)
                        {
                            string dynamicId = $"account-cards-list-mortgage-card-{i}";

                            try
                            {
                                var blockElement = driver.FindElement(By.Id(dynamicId));
                                if (blockElement != null)
                                {
                                    blockElementsWithIndex.Add((blockElement, i, "mortgage"));
                                    Console.WriteLine($"Added Mortgage Block {i}");
                                }
                            }
                            catch (NoSuchElementException ex)
                            {
                                Console.WriteLine($"No element found with ID {dynamicId}: {ex.Message}");
                            }
                        }
                        isMortgageChecked = true;
                    }

                    else if (otherBlocks > 0 && !isOtherChecked)
                    {
                        for (int i = 0; i < otherBlocks; i++)
                        {
                            string dynamicId = $"account-cards-list-other-card-{i}";

                            try
                            {
                                var blockElement = driver.FindElement(By.Id(dynamicId));
                                if (blockElement != null)
                                {
                                    blockElementsWithIndex.Add((blockElement, i, "other"));
                                    Console.WriteLine($"Added Other Block {i}");
                                }
                            }
                            catch (NoSuchElementException ex)
                            {
                                Console.WriteLine($"No element found with ID {dynamicId}: {ex.Message}");
                            }
                        }
                        isOtherChecked = true;
                    }

                    // If all block types are checked, break the loop early
                    if (isRevolvingChecked && isInstallmentChecked && isMortgageChecked && isOtherChecked)
                    {
                        Console.WriteLine("----------------All block types have been processed. Exiting loop.------------------");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log exception details
                Console.WriteLine($"Error during block processing: {ex.Message}");
            }

            // Return the list of blocks with index
            return await Task.FromResult(blockElementsWithIndex);
        }

        private int ExtractBlockCount(IWebDriver driver, By elementBy, string blockType)
        {
            System.Threading.Thread.Sleep(5000);

            int blockCount = 0;

            try
            {
                // Locate the Block Text
                string blockText = driver.FindElement(elementBy).Text;
                Console.WriteLine($"{blockType} Text: " + blockText);

                // Use Regex to extract the number inside parentheses
                var match = Regex.Match(blockText, @"\((\d+)\)");


                if (match.Success)
                {
                    if (int.TryParse(match.Groups[1].Value, out int parsedBlockCount))
                    {
                        blockCount = parsedBlockCount;
                    }
                    else
                    {
                        Console.WriteLine($"Failed to parse the number of {blockType} from text: {blockText}");
                    }
                }
                else
                {
                    Console.WriteLine($"Regex match failed. Could not extract {blockType} count from text: {blockText}");
                }
            }
            catch (NoSuchElementException ex)
            {
                Console.WriteLine($"No element found for {blockType}: {ex.Message}");
            }

            // Return the extracted block count or 0 if not found
            return blockCount;
        }
    }
}
