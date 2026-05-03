using Microsoft.AspNetCore.Mvc;
using PantryCloud.ApiGateway.Presentation.Controllers;
using Shouldly;

namespace PantryCloud.ApiGateway.UnitTests.Controllers;

public class HealthControllerTests
{
    [Fact]
    public void Get_ShouldReturnOk_WithHealthResponse()
    {
        var controller = new HealthController();

        var result = controller.Get();

        var objectResult = result.ShouldBeOfType<OkObjectResult>();
        var response = objectResult.Value.ShouldBeOfType<HealthCheckResponse>();
        response.Status.ShouldBe(Constants.Health.StatusHealthy);
        response.Service.ShouldBe(Constants.Health.ServiceApiGateway);
        response.Timestamp.ShouldNotBe(default);
    }
}
