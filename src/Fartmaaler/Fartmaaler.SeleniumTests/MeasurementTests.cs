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

        driver.Navigate().GoToUrl("https://faartmaalerv2front-haf2cxf9hed7grb7.polandcentral-01.azurewebsites.net/elev.html");

        var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(10));

        wait.Until(d => d.PageSource.Contains("Start en ny session"));

        Assert.Contains("Start en ny session", driver.PageSource);

        driver.Quit();
    }

    [Fact]
    public void Measurement_Form_Fields_Exist()
    {
        IWebDriver driver = CreateDriver();

        driver.Navigate().GoToUrl("https://faartmaalerv2front-haf2cxf9hed7grb7.polandcentral-01.azurewebsites.net/elev.html");

        var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(10));

        Assert.NotNull(wait.Until(d => d.FindElement(By.Id("groupSelect"))));
        Assert.NotNull(wait.Until(d => d.FindElement(By.Id("carTypeSelect"))));
        Assert.NotNull(wait.Until(d => d.FindElement(By.Id("roadTypeSelect"))));

        driver.Quit();
    }

    [Fact]
    public void Measurement_Shows_Error_When_Fields_Are_Empty()
    {
        IWebDriver driver = CreateDriver();

        driver.Navigate().GoToUrl("https://faartmaalerv2front-haf2cxf9hed7grb7.polandcentral-01.azurewebsites.net/elev.html");

        var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(10));

        IWebElement startButton = wait.Until(d =>
            d.FindElement(By.XPath("//button[contains(.,'Start session')]"))
        );

        startButton.Click();

        wait.Until(d => d.PageSource.Contains("Udfyld alle felter"));

        Assert.Contains("Udfyld alle felter", driver.PageSource);

        driver.Quit();
    }
}