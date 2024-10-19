using Microsoft.AspNetCore.Mvc;
using WeatherStationBackend.Services;

namespace WeatherStationBackend.Controllers;

[ApiController]
[Route("api/sensors")]
public class TokenController : ControllerBase
{
    private readonly ILogger<TokenController> _logger;
    private readonly SensorService _sensorService;
    private readonly TokenService _tokenService;

    public TokenController(TokenService tokenService, ILogger<TokenController> logger, SensorService sensorService)
    {
        _tokenService = tokenService;
        _logger = logger;
        _sensorService = sensorService;
    }

    /// <summary>
    ///     Retrieves the token balance for a given sensor ID.
    /// </summary>
    /// <param name="id">The unique identifier of the sensor.</param>
    /// <returns>A JSON object containing the sensor's wallet address and balance in tokens.</returns>
    /// <response code="200">Balance successfully retrieved.</response>
    /// <response code="400">Invalid sensor ID format.</response>
    /// <response code="404">Sensor not found.</response>
    /// <response code="500">Internal server error while retrieving balance.</response>
    [HttpGet("{id}/tokens")]
    public async Task<IActionResult> GetBalance(string id)
    {
        try
        {
            var sensor = await _sensorService.GetAsync(id);
            var sensorAddress = sensor.WalletAddress;
            var tokenAmount = await _tokenService.GetBalanceAsync(sensorAddress);

            return Ok(new { SensorAddress = sensorAddress, Balance = tokenAmount });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (FormatException)
        {
            return BadRequest("Invalid sensor ID format.");
        }
        catch (Exception)
        {
            return StatusCode(500, "Error retrieving balance.");
        }
    }
}