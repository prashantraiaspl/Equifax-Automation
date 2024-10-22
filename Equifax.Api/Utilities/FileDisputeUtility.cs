using Equifax.Api.Domain.DTOs;
using Equifax.Api.Helper;
using OpenQA.Selenium;

namespace Equifax.Api.Utilities
{
    public class FileDisputeUtility
    {
        private readonly ElementLoader _elementLoader;
        private readonly BlocksLoader _blockLoader;
        private readonly BlockMatchingLoader _blockMatchingLoader;
        private readonly CheckboxLoader _checkboxLoader;

        public FileDisputeUtility
            (
                ElementLoader elementLoader, 
                BlocksLoader blocksLoader,
                BlockMatchingLoader blockMatchingLoader,
                CheckboxLoader checkboxLoader
            )
        {
            _elementLoader = elementLoader;
            _blockLoader = blocksLoader;
            _blockMatchingLoader = blockMatchingLoader;
            _checkboxLoader = checkboxLoader;
        }


        public async Task<string> FileDisputeAsync(DisputeRequestDto disputeRequest, IWebDriver driver)
        {
            List<(IWebElement Element, int Index, string Type)> blockElementsWithIndex = new List<(IWebElement Element, int Index, string Type)>();
            string? confirmation_number = null;

            try
            {
                string creditor_name = string.Empty;
                string open_date = string.Empty;
                List<string> reasonArr = new List<string>();
                string comment = string.Empty;


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

                string fileDisputeXPath = "//*[@id=\"file-distupe-section-file-a-dispute-button\"]";
                string checkboxXPath = "//*[@id=\"onlineDeliveryAccept\"]/label/span[1]";
                string continueButtonXPath = "//*[@id=\"ssn-agree-modal-confirm-button\"]";
                string creditAccountXPath = "//*[@id=\"creditAccounts-section-link\"]/i";


                _elementLoader.Load(fileDisputeXPath, driver);
                System.Threading.Thread.Sleep(3000);

                var element = driver.FindElement(By.XPath(checkboxXPath));
                element.Click();
                System.Threading.Thread.Sleep(3000);

                _elementLoader.Load(continueButtonXPath, driver);
                System.Threading.Thread.Sleep(5000);

                _elementLoader.Load(creditAccountXPath, driver);
                System.Threading.Thread.Sleep(3000);


                // Block Loader
                blockElementsWithIndex = await _blockLoader.Process(driver);


                IWebDriver driver_matchedBlock = await _blockMatchingLoader.Matching(blockElementsWithIndex, creditor_name, open_date, driver);

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
                string confirmationNumberXPath = "//*[@id=\"dispute-confirmation-cards-list-confirmation-number\"]/div";

                System.Threading.Thread.Sleep(3000);
                IWebElement commentElement = driver.FindElement(By.XPath(commentXPath));
                commentElement.Clear();
                commentElement.SendKeys(comment);

                _elementLoader.Load(saveBtnXPath, driver);
                _elementLoader.Load(continueBtnXPath, driver);
                _elementLoader.Load(skipUploadXPath, driver);
                _elementLoader.Load(submitDisputeXPath, driver);

                System.Threading.Thread.Sleep(15000);
                var CONFIRMATION_ELEMENT = driver.FindElement(By.XPath(confirmationNumberXPath));
                confirmation_number = CONFIRMATION_ELEMENT.Text;

                if (confirmation_number == null)
                {
                    CONFIRMATION_ELEMENT = driver.FindElement(By.XPath("//*[@id=\"dispute-confirmation-cards-list-confirmation-number\"]/div"));
                    confirmation_number = CONFIRMATION_ELEMENT.Text;
                }

                Console.WriteLine($"confirmation_number: {confirmation_number}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Filing Dispute Error: {ex.Message}");
            }

            return confirmation_number;
        }
    }
}
