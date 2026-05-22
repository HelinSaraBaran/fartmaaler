using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Xunit;

namespace Fartmaaler.SeleniumTests;

public class LoginTests
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
    public void Teacher_Can_Open_Login_Page()
    {
        IWebDriver driver = CreateDriver();

        driver.Navigate().GoToUrl("https://faartmaalerv2front-haf2cxf9hed7grb7.polandcentral-01.azurewebsites.net/teacher-login.html");

        Assert.Contains("Underviser login", driver.PageSource);

        driver.Quit();
    }

    [Fact]
    public void Login_Fields_Exist()
    {
        IWebDriver driver = CreateDriver();

        driver.Navigate().GoToUrl("https://faartmaalerv2front-haf2cxf9hed7grb7.polandcentral-01.azurewebsites.net/teacher-login.html");

        IWebElement usernameInput =
            driver.FindElement(By.Id("usernameInput"));

        IWebElement passwordInput =
            driver.FindElement(By.Id("passwordInput"));

        Assert.NotNull(usernameInput);
        Assert.NotNull(passwordInput);

        driver.Quit();
    }

    [Fact]
    public void Teacher_Can_Type_In_Login_Fields()
    {
        IWebDriver driver = CreateDriver();

        driver.Navigate().GoToUrl("https://faartmaalerv2front-haf2cxf9hed7grb7.polandcentral-01.azurewebsites.net/teacher-login.html");

        IWebElement usernameInput =
            driver.FindElement(By.Id("usernameInput"));

        IWebElement passwordInput =
            driver.FindElement(By.Id("passwordInput"));

        usernameInput.SendKeys("test");

        passwordInput.SendKeys("1234");

        Assert.Equal(
            "test",
            usernameInput.GetAttribute("value")
        );

        Assert.Equal(
            "1234",
            passwordInput.GetAttribute("value")
        );

        driver.Quit();
    }
}