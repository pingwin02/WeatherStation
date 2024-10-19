using System.Numerics;
using Microsoft.Extensions.Options;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Util;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using WeatherStationBackend.Configuration;

namespace WeatherStationBackend.Services;

[Function("transfer", "bool")]
public class TransferFunction : FunctionMessage
{
    [Parameter("address", "_to")] public string To { get; set; }

    [Parameter("uint256", "_value", 2)] public BigInteger TokenAmount { get; set; }
}

public class TokenService
{
    private readonly string _abi;
    private readonly BigInteger _award;
    private readonly string _contractAddress;
    private readonly ILogger<TokenService> _logger;
    private readonly Web3 _web3;

    public TokenService(
        IOptions<TokenSettings> tokenSettings,
        ILogger<TokenService> logger)
    {
        var account = new Account(tokenSettings.Value.PrivateKey);
        _web3 = new Web3(account, tokenSettings.Value.InfuraUrl);
        _contractAddress = tokenSettings.Value.ContractAddress;
        _abi = tokenSettings.Value.Abi;
        _award = BigInteger.Parse(tokenSettings.Value.Award);
        _logger = logger;
        _logger.LogInformation("TokenService initialized");
    }

    public async Task<BigInteger> GetBalanceAsync(string address)
    {
        var contract = _web3.Eth.GetContract(_abi, _contractAddress);
        var balanceOfFunction = contract.GetFunction("balanceOf");
        var balance = await balanceOfFunction.CallAsync<BigInteger>(address);
        return balance;
    }

    public async Task TransferTokensAsync(string toAddress, BigInteger? amount = null)
    {
        try
        {
            _logger.LogInformation($"Initiating transfer to {toAddress} with amount {amount ?? _award}");
            var transferHandler = _web3.Eth.GetContractTransactionHandler<TransferFunction>();
            var transfer = new TransferFunction
            {
                To = toAddress,
                TokenAmount = amount ?? _award
            };
            transfer.GasPrice = Web3.Convert.ToWei(25, UnitConversion.EthUnit.Gwei);
            var estimatedGas = await transferHandler.EstimateGasAsync(_contractAddress, transfer);
            transfer.Gas = estimatedGas.Value;

            _logger.LogInformation("Sending transaction...");
            var transactionReceipt =
                await transferHandler.SendRequestAndWaitForReceiptAsync(_contractAddress, transfer);

            var receiptHash = transactionReceipt.TransactionHash;
            _logger.LogInformation($"Transaction hash: {receiptHash}");

            _logger.LogInformation("Transfer successful");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to transfer tokens to {toAddress}: {ex.Message}");
        }
    }
}