using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Xunit;

namespace Fartmaaler.SeleniumTests;

public class MeasurementTests
{
    private IWebDriver CreateDriver()
    {
        ChromeOptions options = new ChromeOptions();

        options.AddArgument("--headless=new");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--window-size=1920,1080");

        IWebDriver driver = new ChromeDriver(options);

        return driver;
    }

    [Fact]
    public void Measurement_Page_Loads()
    {
        IWebDriver driver = CreateDriver();

        driver.Navigate().GoToUrl("http://127.0.0.1:5500/elev.html");

        Assert.Contains("Start en ny session", driver.PageSource);

        driver.Quit();
    }

    [Fact]
    public void Measurement_Form_Fields_Exist()
    {
        IWebDriver driver = CreateDriver();

        driver.Navigate().GoToUrl("http://127.0.0.1:5500/elev.html");

        Assert.NotNull(driver.FindElement(By.Id("groupSelect")));
        Assert.NotNull(driver.FindElement(By.Id("carTypeSelect")));
        Assert.NotNull(driver.FindElement(By.Id("roadTypeSelect")));

        driver.Quit();
    }

    [Fact]
    public void Measurement_Shows_Error_When_Fields_Are_Empty()
    {
        IWebDriver driver = CreateDriver();

        driver.Navigate().GoToUrl("http://127.0.0.1:5500/elev.html");

        IWebElement startButton = driver.FindElement(
            By.XPath("//button[contains(text(),'Start session')]")
        );

        startButton.Click();

        Assert.Contains("Udfyld alle felter", driver.PageSource);

        driver.Quit();
    }
}