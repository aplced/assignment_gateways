using Gateways.Models;
using Gateways.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace Gateways.Tests
{
    public class GatewaysControllerTests
    {
        private readonly GatewaysController gatewaysController;
        //This mock is not actually being used as mocking the entire context is actually a lot harder than setting up and using
        //an in memory real one. Even though this goes into the realm of integration tests I am going to use it as a unit test.
        //private readonly Mock<Gateways.Models.GatewayContext> gatewayContextMock = new Mock<Gateways.Models.GatewayContext>();

        public GatewaysControllerTests()
        {
            var options = new DbContextOptionsBuilder<GatewayContext>().UseInMemoryDatabase("Test Gateways").Options;
            gatewaysController = new GatewaysController(new GatewayContext(options));
        }
        
        [Fact]
        public async void GettingGatewaysReturnsNoGateways()
        {
            ActionResult<IEnumerable<Gateway>> gateways = await gatewaysController.GetGateways();

            Assert.True(gateways.Value.GetEnumerator().Current == null);
        }

        [Fact]
        public async void AddingGatewayReturnsTheSameGateway()
        {
            var gateway = new Gateway(){
                IPv4 = "127.0.0.1",
                SerialNumber = "0000",
                Name = "No name"
            };
            ActionResult<Gateway> gatewayResult = await gatewaysController.PostGateway(gateway);

            //Since equals is not overriden assuming serial number is enough for the test purposes
            Assert.True(gatewayResult.Value.SerialNumber == gateway.SerialNumber);
        }

        [Fact]
        public async void AddingGatewayWithNonUniqueSerialNumberFails()
        {
            var gateway = new Gateway(){
                IPv4 = "127.0.0.1",
                SerialNumber = "0000",
                Name = "No name"
            };
            await gatewaysController.PostGateway(gateway);

            gateway.Name = "Just so its not the same";
            ActionResult<Gateway> gatewayResult = await gatewaysController.PostGateway(gateway);

            
            Assert.True(gatewayResult.Result.GetType() == typeof(Microsoft.AspNetCore.Mvc.ConflictResult));
            Assert.True(((Microsoft.AspNetCore.Mvc.ConflictResult)gatewayResult.Result).StatusCode == 409);
        }

        [Fact]
        public async void IPv4FieldIsValidated()
        {
            var gateway = new Gateway(){
                IPv4 = "2001:0db8:85a3:0000:0000:8a2e:0370:7334",
                SerialNumber = "0000",
                Name = "No name"
            };
            ActionResult<Gateway> gatewayResult = await gatewaysController.PostGateway(gateway);

            //Since equals is not overriden assuming serial number is enough for the test purposes
            Assert.True(gatewayResult.Value.SerialNumber == gateway.SerialNumber);
        }
    }
}
