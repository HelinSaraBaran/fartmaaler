using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Xunit;

namespace Fartmaaler.SeleniumTests;

public class LoginTests
{
    [Fact]
    public void Teacher_Can_Open_Login_Page()
    {
        IWebDriver driver = new ChromeDriver();

        driver.Navigate().GoToUrl("http://127.0.0.1:5500/teacher-login.html");

        Assert.Contains("Underviser login", driver.PageSource);

        driver.Quit();
    }

    [Fact]
    public void Login_Fields_Exist()
    {
        IWebDriver driver = new ChromeDriver();

        driver.Navigate().GoToUrl("http://127.0.0.1:5500/teacher-login.html");

        var usernameInput =
            driver.FindElement(By.Id("usernameInput"));

        var passwordInput =
            driver.FindElement(By.Id("passwordInput"));

        Assert.NotNull(usernameInput);
        Assert.NotNull(passwordInput);

        driver.Quit();
    }

    [Fact]
    public void Teacher_Can_Type_In_Login_Fields()
    {
        IWebDriver driver = new ChromeDriver();

        driver.Navigate().GoToUrl("http://127.0.0.1:5500/teacher-login.html");

        var usernameInput =
            driver.FindElement(By.Id("usernameInput"));

        var passwordInput =
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