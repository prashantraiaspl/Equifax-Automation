using AngleSharp.Dom;
using OpenQA.Selenium;

namespace Equifax.Api.Helper
{
    public class ElementLoader
    {
        public void Load(string xPath, IWebDriver driver)
        {
			try
			{
                var elements = driver.FindElements(By.XPath(xPath));

                if (elements.Count > 0)
                {
                    elements[0].Click();
                }
                else
                {
                    System.Threading.Thread.Sleep(10000);
                    var element = driver.FindElement(By.XPath(xPath));
                    System.Threading.Thread.Sleep(10000);
                    element.Click();
                }
            }
			catch (Exception ex)
			{
                Console.WriteLine($"Element Loading Error: {ex.Message}");
            }
        }
    }
}
