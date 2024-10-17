using Microsoft.AspNetCore.Mvc;
using WeatherStationBackend.Models;
using WeatherStationBackend.Services;

namespace WeatherStationBackend.Controllers;

[ApiController]
[Route("api")]
public class DataController : ControllerBase
{
    private readonly DataService _dataService;
    private readonly SensorService _sensorService;

    public DataController(DataService dataService, SensorService sensorService)
    {
        _dataService = dataService;
        _sensorService = sensorService;
    }

    /// <summary>
    ///     Gets all sensor data records.
    /// </summary>
    /// <returns>A list of all sensor data.</returns>
    /// <response code="200">Returns the list of sensor data</response>
    /// <response code="500">If there is an internal error</response>
    [HttpGet]
    [Route("data")]
    public async Task<ActionResult<List<DataEntity>>> GetAllData()
    {
        try
        {
            var data = await _dataService.GetAllAsync();
            return Ok(data);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    /// <summary>
    ///     Gets data by sensor ID.
    /// </summary>
    /// <param name="id">The ID of the sensor.</param>
    /// <returns>Data related to the given sensor ID.</returns>
    /// <response code="200">Returns the data related to the sensor</response>
    /// <response code="400">If the sensor ID format is invalid</response>
    /// <response code="404">If the sensor is not found</response>
    /// <response code="500">If there is an internal error</response>
    [HttpGet("sensors/{id}/data")]
    public async Task<ActionResult<List<DataEntity>>> GetDataBySensorId(string id)
    {
        try
        {
            await _sensorService.GetAsync(id);
            var data = await _dataService.GetBySensorIdAsync(id);
            return Ok(data);
        }
        catch (FormatException e)
        {
            return BadRequest(e.Message);
        }
        catch (KeyNotFoundException e)
        {
            return NotFound();
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    /// <summary>
    ///     Gets the most recent data for a specific sensor ID.
    /// </summary>
    /// <param name="id">The ID of the sensor.</param>
    /// <returns>The most recent data related to the sensor ID.</returns>
    /// <response code="200">Returns the most recent data related to the sensor</response>
    /// <response code="400">If the sensor ID format is invalid</response>
    /// <response code="404">If no data is found for the sensor</response>
    /// <response code="500">If there is an internal error</response>
    [HttpGet("sensors/{id}/data/recent")]
    public async Task<ActionResult<DataEntity>> GetMostRecentDataBySensorId(string id)
    {
        try
        {
            await _sensorService.GetAsync(id);
            var data = await _dataService.GetMostRecentBySensorIdAsync(id);
            if (data == null) return NotFound();
            return Ok(data);
        }
        catch (FormatException e)
        {
            return BadRequest(e.Message);
        }
        catch (KeyNotFoundException e)
        {
            return NotFound();
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }
}