namespace WeatherStationBackend.Configuration;

public class TokenSettings
{
    public string Abi { get; set; } = null!;
    public string ContractAddress { get; set; } = null!;
    public string PrivateKey { get; set; } = null!;
    public string InfuraUrl { get; set; } = null!;
    public string Award { get; set; } = null!;
}