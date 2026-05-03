using Microsoft.AspNetCore.Mvc;

namespace PantryCloud.ApiGateway.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(HealthCheckResponse), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok(new HealthCheckResponse
        {
            Status = "Healthy",
            Service = "ApiGateway",
            Timestamp = DateTime.UtcNow
        });
    }
}

public class HealthCheckResponse
{
    public string Status { get; set; } = string.Empty;
    public string Service { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

