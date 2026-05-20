using FartmaalerAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace FartmaalerAPI.Tests
{
    public class FunFactsControllerTests
    {
        [Fact]
        // Tester at GetRandomFunFact returnerer Ok
        public void GetRandomFunFact_ReturnsOk()
        {
            var controller = new FunFactsController();

            var result = controller.GetRandomFunFact();

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        // Tester at GetAllFunFacts returnerer Ok
        public void GetAllFunFacts_ReturnsOk()
        {
            var controller = new FunFactsController();

            var result = controller.GetAllFunFacts();

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        // Tester at GetAllFunFacts returnerer en liste med fun facts
        public void GetAllFunFacts_ReturnsListOfFunFacts()
        {
            var controller = new FunFactsController();

            var result = controller.GetAllFunFacts();

            var okResult = Assert.IsType<OkObjectResult>(result);

            var funFacts = Assert.IsType<List<string>>(okResult.Value);

            Assert.NotEmpty(funFacts);
        }

        [Fact]
        // Tester at der findes 5 fun facts
        public void GetAllFunFacts_ReturnsFiveFunFacts()
        {
            var controller = new FunFactsController();

            var result = controller.GetAllFunFacts();

            var okResult = Assert.IsType<OkObjectResult>(result);

            var funFacts = Assert.IsType<List<string>>(okResult.Value);

            Assert.Equal(5, funFacts.Count);
        }
    }
}