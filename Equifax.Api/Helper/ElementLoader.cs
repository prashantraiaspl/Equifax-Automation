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
                    Console.WriteLine("------ELement is Clicked (if).------");
                }
                else
                {
                    System.Threading.Thread.Sleep(10000);
                    var element = driver.FindElement(By.XPath(xPath));
                    System.Threading.Thread.Sleep(10000);
                    element.Click();
                    Console.WriteLine("------ELement is Clicked (else).------");
                }
            }
			catch (Exception ex)
			{
                Console.WriteLine($"Element Loading Error: {ex.Message}");
            }
        }
    }
}
