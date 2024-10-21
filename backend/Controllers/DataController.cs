using Microsoft.AspNetCore.Mvc;
using WeatherStationBackend.Models;
using WeatherStationBackend.Services;

namespace WeatherStationBackend.Controllers;

[ApiController]
[Route("api")]
public class DataController : ControllerBase
{
    private readonly DataService _dataService;

    public DataController(DataService dataService)
    {
        _dataService = dataService;
    }

    /// <summary>
    ///     Gets filtered sensor data records.
    /// </summary>
    /// <param name="sensorType">The type of sensor (optional).</param>
    /// <param name="sensorId">The ID of the sensor (optional).</param>
    /// <param name="sensorName">The name of the sensor (optional).</param>
    /// <param name="startDate">The start date for filtering data (optional).</param>
    /// <param name="endDate">The end date for filtering data (optional).</param>
    /// <param name="limit">The maximum number of records to return (optional).</param>
    /// <param name="sortBy">The field to sort by (optional).</param>
    /// <param name="sortOrder">The sort order (optional): `asc` or `desc`.</param>
    /// <param name="export">The format to export the data (optional): `csv` or `json`.</param>
    /// <response code="200">
    ///     Returns the list of sensor data.
    ///     If the `export` query parameter is set to `csv` or `json`, a file download will be triggered.
    /// </response>
    /// <response code="400">If the query parameters are invalid.</response>
    /// <response code="500">If there is an internal server error.</response>
    [HttpGet]
    [Route("data")]
    public async Task<ActionResult<List<DataEntity>>> GetAllData(
        [FromQuery] string? sensorType = null,
        [FromQuery] string? sensorId = null,
        [FromQuery] string? sensorName = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? limit = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = null,
        [FromQuery] string? export = null)
    {
        try
        {
            var data = await _dataService.GetFilteredDataAsync(
                sensorType,
                sensorId,
                sensorName,
                startDate,
                endDate,
                limit,
                sortBy,
                sortOrder);

            if (export != null
                && !string.Equals(export, "csv", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(export, "json", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Invalid export type. Please use 'csv' or 'json'.");

            if (string.Equals(export, "csv", StringComparison.OrdinalIgnoreCase))
            {
                var csvStream = await _dataService.ExportToCsvAsync(data);
                return File(csvStream, "text/csv");
            }

            if (string.Equals(export, "json", StringComparison.OrdinalIgnoreCase))
            {
                var jsonStream = await _dataService.ExportToJsonAsync(data);
                return File(jsonStream, "application/json");
            }

            return Ok(data);
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
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