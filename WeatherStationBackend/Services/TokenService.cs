using Nethereum.Web3;
using Nethereum.Contracts;
using Nethereum.Web3.Accounts;
using System.Numerics;
using Microsoft.Extensions.Options;
using Nethereum.ABI.FunctionEncoding.Attributes;
using WeatherStationBackend.Configuration;


namespace WeatherStationBackend.Services
{
    
    [Function("transfer", "bool")]
    public class TransferFunction : FunctionMessage
    {
        [Parameter("address", "_to", 1)]
        public string To { get; set; }
        [Parameter("uint256", "_value", 2)]
        public BigInteger TokenAmount { get; set; }
    }
    
    public class TokenService
    {
        private readonly Web3 _web3;
        private readonly ILogger<TokenService> _logger;
        private readonly string _contractAddress;
        private readonly string _abi;
        private readonly int _decimals = 6;

        public TokenService(
            IOptions<TokenSettings> tokenSettings,
            ILogger<TokenService> logger)
        {
            
            
            var account = new Account(tokenSettings.Value.PrivateKey);
            _web3 = new Web3(account, tokenSettings.Value.InfuraUrl);
            _contractAddress = tokenSettings.Value.ContractAddress;
            _abi = tokenSettings.Value.Abi;
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

        public async Task<bool> TransferTokensAsync(string toAddress, BigInteger amount)
        {
            try
            {
                var transferHandler = _web3.Eth.GetContractTransactionHandler<TransferFunction>();
                var transfer = new TransferFunction
                {
                    To = toAddress,
                    TokenAmount = amount
                };

                var transactionReceipt = await transferHandler.SendRequestAndWaitForReceiptAsync(_contractAddress, transfer);
                _logger.LogInformation($"Transferred {amount} tokens to {toAddress}");
                return transactionReceipt.Status.Value == 1;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to transfer tokens: {ex.Message}");
                return false;
            }
        }
    }
}
