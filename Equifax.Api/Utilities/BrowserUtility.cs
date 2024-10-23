using OpenQA.Selenium;
using Equifax.Api.Domain.DTOs;
using Equifax.Api.Helper;

namespace Equifax.Api.Utilities
{
    public class BrowserUtility
    {
        private readonly DriverSetupManager _driverSetupManager;
        private readonly OpenBrowserAndNavigateUtility _openBrowserAndNavigateUtility;
        private readonly LoginUtility _loginUtility;
        private readonly NavigateToDisputeUtility _navigateToDisputeUtility;
        private readonly FileDisputeUtility _fileDisputeUtility;
        private readonly CloseBrowserUtility _closeBrowserUtility;
        private readonly SleepLoader _sleepLoader;

        public BrowserUtility
            (   DriverSetupManager driverSetupManager,
                OpenBrowserAndNavigateUtility openBrowserAndNavigateUtility,
                LoginUtility loginUtility,
                NavigateToDisputeUtility navigateToDisputeUtility,
                FileDisputeUtility fileDisputeUtility,
                CloseBrowserUtility closeBrowserUtility,
                SleepLoader sleepLoader
            )
        {
            _driverSetupManager = driverSetupManager;
            _openBrowserAndNavigateUtility = openBrowserAndNavigateUtility;
            _loginUtility = loginUtility;
            _navigateToDisputeUtility = navigateToDisputeUtility;
            _fileDisputeUtility = fileDisputeUtility;
            _closeBrowserUtility = closeBrowserUtility;
            _sleepLoader = sleepLoader;
        }



        public async Task<string> BrowserAutomationProcess(string url, LoginCredentialRequestDto loginCredentials, DisputeRequestDto disputeRequest)
        {
            string confirmationNumber = "";

            try
            {
                IWebDriver? driver = null;

                //-------------INITIALIZATION OF CHROME DRIVER-------------//
                driver = _driverSetupManager.InitializeDriver();

                _sleepLoader.Seconds(3);

                //-------------Step 1: Open URL-------------//
                _openBrowserAndNavigateUtility.OpenBrowserAndNavigate(url, driver);

                //-------------Step 2: Perform Login-------------//
                _loginUtility.Login(loginCredentials, driver);

                _sleepLoader.Seconds(5);

                //-------------Step 3: Navigate to Dispute-------------//
                _navigateToDisputeUtility.NavigateToDispute(driver);

                _sleepLoader.Seconds(5);

                //-------------Step 4: File a Dispute-------------//
                confirmationNumber = await _fileDisputeUtility.FileDisputeAsync(disputeRequest, driver);

                //-------------Step 5: Close the Browser-------------//
                //_closeBrowserUtility.CloseBrowser(driver);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during browser interaction: {ex.Message}");
            }

            return confirmationNumber;
        }
    }

}
