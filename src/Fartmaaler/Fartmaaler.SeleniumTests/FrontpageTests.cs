using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Xunit;

namespace Fartmaaler.SeleniumTests;

public class FrontpageTests
{
    [Fact]
    public void Frontpage_Loads()
    {
        IWebDriver driver = new ChromeDriver();

        driver.Navigate().GoToUrl("http://127.0.0.1:5500/index.html");

        Assert.Contains("Smart Fartmåler", driver.PageSource);

        driver.Quit();
    }

    [Fact]
    public void Teacher_Login_Button_Can_Click()
    {
        IWebDriver driver = new ChromeDriver();

        driver.Navigate().GoToUrl("http://127.0.0.1:5500/index.html");

        var loginButton = driver.FindElement(
            By.XPath("//button[contains(text(), 'Underviser login')]")
        );

        loginButton.Click();

        Assert.Contains("login", driver.Url.ToLower());

        driver.Quit();
    }

    [Fact]
    public void Start_Measurement_Button_Can_Click()
    {
        IWebDriver driver = new ChromeDriver();

        driver.Navigate().GoToUrl("http://127.0.0.1:5500/index.html");

        var startButton = driver.FindElement(
            By.XPath("//button[contains(text(), 'Start din måling')]")
        );

        startButton.Click();

        Assert.DoesNotContain("index.html", driver.Url.ToLower());

        driver.Quit();
    }
}