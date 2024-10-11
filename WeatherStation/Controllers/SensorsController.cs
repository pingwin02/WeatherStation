namespace WeatherStation.Controllers;
using WeatherStation.Models;
using WeatherStation.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class SensorsController : ControllerBase
{
    private readonly SensorService _sensorService;

    public SensorsController(SensorService sensorService) =>
        _sensorService = sensorService;

    [HttpGet]
    public async Task<List<Sensor>> Get() =>
        await _sensorService.GetAsync();

    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<Sensor>> Get(string id)
    {
        var book = await _sensorService.GetAsync(id);

        if (book is null)
        {
            return NotFound();
        }

        return book;
    }
    
    [HttpGet("recent/{sensorNumber:int}")]
    public async Task<ActionResult<Sensor>> GetMostRecentSensor(int sensorNumber)
    {
        try
        {
            var sensor = await _sensorService.GetMostRecentSensorByNumberAsync(sensorNumber);
            if (sensor is null)
            {
                return NotFound();
            }
            return sensor;
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Post(Sensor newSensor)
    {
        await _sensorService.CreateAsync(newSensor);

        return CreatedAtAction(nameof(Get), new { id = newSensor.Id }, newSensor);
    }

    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> Update(string id, Sensor updatedBook)
    {
        var book = await _sensorService.GetAsync(id);

        if (book is null)
        {
            return NotFound();
        }

        updatedBook.Id = book.Id;

        await _sensorService.UpdateAsync(id, updatedBook);

        return NoContent();
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
        var book = await _sensorService.GetAsync(id);

        if (book is null)
        {
            return NotFound();
        }

        await _sensorService.RemoveAsync(id);

        return NoContent();
    }
}