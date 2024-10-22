using OpenQA.Selenium;

namespace Equifax.Api.Utilities
{
    public class CloseBrowserUtility
    {
        public void CloseBrowser(IWebDriver driver)
        {
            driver.Quit();
        }
    }
}
