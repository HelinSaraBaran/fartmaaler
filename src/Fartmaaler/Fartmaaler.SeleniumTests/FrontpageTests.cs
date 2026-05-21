using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Xunit;

namespace Fartmaaler.SeleniumTests;

public class FrontpageTests
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
    public void Frontpage_Loads()
    {
        IWebDriver driver = CreateDriver();

        driver.Navigate().GoToUrl("http://127.0.0.1:5500/index.html");

        Assert.Contains("Smart Fartmåler", driver.PageSource);

        driver.Quit();
    }

    [Fact]
    public void Teacher_Login_Button_Can_Click()
    {
        IWebDriver driver = CreateDriver();

        driver.Navigate().GoToUrl("http://127.0.0.1:5500/index.html");

        IWebElement loginButton = driver.FindElement(
            By.XPath("//button[contains(., 'Underviser login')]")
        );

        loginButton.Click();

        Assert.Contains("login", driver.Url.ToLower());

        driver.Quit();
    }

    [Fact]
    public void Start_Measurement_Button_Can_Click()
    {
        IWebDriver driver = CreateDriver();

        driver.Navigate().GoToUrl("http://127.0.0.1:5500/index.html");

        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

        IWebElement startButton = wait.Until(d =>
            d.FindElement(By.XPath("//button[contains(., 'Start din måling')]"))
        );

        startButton.Click();

        Assert.DoesNotContain("index.html", driver.Url.ToLower());

        driver.Quit();
    }
}