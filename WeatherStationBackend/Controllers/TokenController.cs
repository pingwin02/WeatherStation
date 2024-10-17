using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WeatherStationBackend.Services;
using Microsoft.Extensions.Logging;
using System.Numerics;
using Nethereum.Web3;

namespace WeatherStationBackend.Controllers
{
    [ApiController]
    [Route("api/tokens")]
    public class TokenController : ControllerBase
    {
        private readonly TokenService _tokenService;
        private readonly DataService _dataService;
        private readonly ILogger<TokenController> _logger;
        const int Decimals = 6;

        public TokenController(TokenService tokenService, ILogger<TokenController> logger, DataService dataService)
        {
            _tokenService = tokenService;
            _logger = logger;
            _dataService = dataService;
        }

        // GET: api/tokens/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBalance(string id)
        {
            var sensorAddress = await _dataService.GetSensorById(id).Address;

            try
            {
                BigInteger balance = await _tokenService.GetBalanceAsync(sensorAddress);
                var tokenAmount = Web3.Convert.FromWei(balance, Decimals);

                _logger.LogInformation($"Balance for sensor {sensorAddress} is {tokenAmount} tokens.");
                return Ok(new { SensorAddress = sensorAddress, Balance = tokenAmount });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving balance for sensor {sensorAddress}: {ex.Message}");
                return StatusCode(500, "Error retrieving balance.");
            }
        }
    }
}