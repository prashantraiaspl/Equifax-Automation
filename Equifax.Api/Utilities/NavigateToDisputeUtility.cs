using Equifax.Api.Helper;
using OpenQA.Selenium;

namespace Equifax.Api.Utilities
{
    public class NavigateToDisputeUtility
    {
        private readonly ElementLoader _elementLoader;

        public NavigateToDisputeUtility(ElementLoader elementLoader)
        {
            _elementLoader = elementLoader;
        }


        public void NavigateToDispute(IWebDriver driver)
        {
            try
            {
                string disputeCenterXPath = "//*[@id='fullDisputeLink']";

                _elementLoader.Load(disputeCenterXPath, driver);

                Console.WriteLine("Navigated to dispute tab.");
                System.Threading.Thread.Sleep(3000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Navigation Error: {ex.Message}");
            }
        }
    }
}
