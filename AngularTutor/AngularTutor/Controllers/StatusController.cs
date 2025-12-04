using Microsoft.AspNetCore.Mvc;

namespace AngularTutor.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatusController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            service = "AngularTutor API",
            status = "online",
            timestamp = DateTime.UtcNow,
        });
    }
}
