using OpenQA.Selenium;

namespace Equifax.Api.Helper
{
    public class CheckboxLoader
    {
        public void CheckboxHandelling(string xPath, IWebDriver driver, List<string> reasonArr)
        {
            try
            {
                var checkBoxText = driver.FindElement(By.XPath(xPath)).Text.Trim().ToLower();

                foreach (var reason in reasonArr)
                {
                    if (checkBoxText.Equals(reason.Trim().ToLower()))
                    {
                        driver.FindElement(By.XPath(xPath)).Click();
                        Console.WriteLine("------CheckBox Clicked.------");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Checkbox Loading Error: {ex.Message}");
            }
        }
    }
}
