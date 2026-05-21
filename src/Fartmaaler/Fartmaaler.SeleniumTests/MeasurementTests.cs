using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Xunit;

namespace Fartmaaler.SeleniumTests;

public class MeasurementTests
{
    [Fact]
    public void Measurement_Page_Loads()
    {
        IWebDriver driver = new ChromeDriver();

        driver.Navigate().GoToUrl("http://127.0.0.1:5500/elev.html");

        Assert.Contains("Start en ny session", driver.PageSource);

        driver.Quit();
    }

    [Fact]
    public void Measurement_Form_Fields_Exist()
    {
        IWebDriver driver = new ChromeDriver();

        driver.Navigate().GoToUrl("http://127.0.0.1:5500/elev.html");

        Assert.NotNull(driver.FindElement(By.Id("groupSelect")));
        Assert.NotNull(driver.FindElement(By.Id("carTypeSelect")));
        Assert.NotNull(driver.FindElement(By.Id("roadTypeSelect")));

        driver.Quit();
    }

    [Fact]
    public void Measurement_Shows_Error_When_Fields_Are_Empty()
    {
        IWebDriver driver = new ChromeDriver();

        driver.Navigate().GoToUrl("http://127.0.0.1:5500/elev.html");

        var startButton = driver.FindElement(
            By.XPath("//button[contains(text(),'Start session')]")
        );

        startButton.Click();

        Assert.Contains("Udfyld alle felter", driver.PageSource);

        driver.Quit();
    }
}