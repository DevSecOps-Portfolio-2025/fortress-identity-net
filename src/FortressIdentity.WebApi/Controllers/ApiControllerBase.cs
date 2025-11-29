using Microsoft.AspNetCore.Mvc;

namespace FortressIdentity.WebApi.Controllers;

/// <summary>
/// Base class for all API controllers.
/// Provides common attributes and configuration.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
}
