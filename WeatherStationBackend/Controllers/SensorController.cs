using Microsoft.AspNetCore.Mvc;
using WeatherStationBackend.Models;
using WeatherStationBackend.Services;

namespace WeatherStationBackend.Controllers;

[ApiController]
[Route("api/sensors")]
public class SensorController : ControllerBase
{
    private readonly DataService _dataService;
    private readonly SensorService _sensorService;
    private readonly TokenService _tokenService;

    public SensorController(SensorService sensorService, DataService dataService, TokenService tokenService)
    {
        _sensorService = sensorService;
        _dataService = dataService;
        _tokenService = tokenService;
    }

    /// <summary>
    ///     Gets all sensors.
    /// </summary>
    /// <returns>A list of all sensors.</returns>
    /// <response code="200">Returns the list of sensors.</response>
    /// <response code="500">If there is an internal server error.</response>
    [HttpGet]
    public async Task<ActionResult<List<SensorEntity>>> GetAllSensors()
    {
        try
        {
            var sensors = await _sensorService.GetAsync();
            return Ok(sensors);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    /// <summary>
    ///     Gets a specific sensor by ID.
    /// </summary>
    /// <param name="id">The sensor ID.</param>
    /// <returns>The sensor entity corresponding to the ID.</returns>
    /// <response code="200">Returns the sensor entity.</response>
    /// <response code="404">If the sensor with the specified ID is not found.</response>
    /// <response code="400">If the ID format is invalid.</response>
    /// <response code="500">If there is an internal server error.</response>
    [HttpGet("{id}")]
    public async Task<ActionResult<SensorEntity>> GetSensorById(string id)
    {
        try
        {
            var sensor = await _sensorService.GetAsync(id);
            return Ok(sensor);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (FormatException e)
        {
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    /// <summary>
    ///     Creates a new sensor.
    /// </summary>
    /// <param name="newSensor">The sensor details to create.</param>
    /// <returns>HTTP 201 Created if successful, or an error status.</returns>
    /// <response code="201">Returns the location of the created sensor.</response>
    /// <response code="400">If the sensor data is invalid.</response>
    /// <response code="500">If there is an internal server error.</response>
    [HttpPost]
    public async Task<ActionResult> CreateSensor([FromBody] SensorRequest newSensor)
    {
        try
        {
            var id = await _sensorService.CreateAsync(newSensor);
            return CreatedAtAction(nameof(GetSensorById), new { id }, null);
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    /// <summary>
    ///     Updates an existing sensor.
    /// </summary>
    /// <param name="id">The sensor ID to update.</param>
    /// <param name="updatedSensor">The updated sensor details.</param>
    /// <returns>HTTP 204 No Content if successful, or an error status.</returns>
    /// <response code="204">If the sensor is updated successfully.</response>
    /// <response code="404">If the sensor with the specified ID is not found.</response>
    /// <response code="400">If the ID format is invalid.</response>
    /// <response code="500">If there is an internal server error.</response>
    [HttpPatch("{id}")]
    public async Task<ActionResult> UpdateSensor(string id, [FromBody] SensorRequest updatedSensor)
    {
        try
        {
            await _sensorService.UpdateAsync(id, updatedSensor);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (FormatException e)
        {
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    /// <summary>
    ///     Deletes a specific sensor by ID.
    /// </summary>
    /// <param name="id">The sensor ID to delete.</param>
    /// <returns>HTTP 204 No Content if successful, or an error status.</returns>
    /// <response code="204">If the sensor is deleted successfully.</response>
    /// <response code="404">If the sensor with the specified ID is not found.</response>
    /// <response code="400">If the ID format is invalid.</response>
    /// <response code="500">If there is an internal server error.</response>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteSensor(string id)
    {
        try
        {
            await _dataService.DeleteBySensorIdAsync(id);
            await _sensorService.RemoveAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (FormatException e)
        {
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }
}