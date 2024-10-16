using Nethereum.Web3;
using Nethereum.Contracts;
using System.Numerics;
using Nethereum.Web3.Accounts;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace NethereumSample
{

    [Function("transfer", "bool")]
    public class TransferFunction : FunctionMessage
    {
        [Parameter("address", "_to", 1)]
        public string To { get; set; }

        [Parameter("uint256", "_value", 2)]
        public BigInteger TokenAmount { get; set; }
    }

    class Program
    {
        private static string contractAddress = "0xe99DDc1405e2a5C2C4D57642Ea742706A9dDB750";
        private static string abi = @"[{""inputs"":[],""stateMutability"":""nonpayable"",""type"":""constructor""},{""anonymous"":false,""inputs"":[{""indexed"":true,""internalType"":""address"",""name"":""owner"",""type"":""address""},{""indexed"":true,""internalType"":""address"",""name"":""spender"",""type"":""address""},{""indexed"":false,""internalType"":""uint256"",""name"":""value"",""type"":""uint256""}],""name"":""Approval"",""type"":""event""},{""inputs"":[{""internalType"":""address"",""name"":""spender"",""type"":""address""},{""internalType"":""uint256"",""name"":""amount"",""type"":""uint256""}],""name"":""approve"",""outputs"":[{""internalType"":""bool"",""name"":""success"",""type"":""bool""}],""stateMutability"":""nonpayable"",""type"":""function""},{""inputs"":[{""internalType"":""address"",""name"":""recipient"",""type"":""address""},{""internalType"":""uint256"",""name"":""amount"",""type"":""uint256""}],""name"":""transfer"",""outputs"":[{""internalType"":""bool"",""name"":""success"",""type"":""bool""}],""stateMutability"":""nonpayable"",""type"":""function""},{""anonymous"":false,""inputs"":[{""indexed"":true,""internalType"":""address"",""name"":""from"",""type"":""address""},{""indexed"":true,""internalType"":""address"",""name"":""to"",""type"":""address""},{""indexed"":false,""internalType"":""uint256"",""name"":""value"",""type"":""uint256""}],""name"":""Transfer"",""type"":""event""},{""inputs"":[{""internalType"":""address"",""name"":""from"",""type"":""address""},{""internalType"":""address"",""name"":""to"",""type"":""address""},{""internalType"":""uint256"",""name"":""amount"",""type"":""uint256""}],""name"":""transferFrom"",""outputs"":[{""internalType"":""bool"",""name"":""success"",""type"":""bool""}],""stateMutability"":""nonpayable"",""type"":""function""},{""inputs"":[],""name"":""_totalSupply"",""outputs"":[{""internalType"":""uint256"",""name"":"""",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function""},{""inputs"":[{""internalType"":""address"",""name"":""ownner"",""type"":""address""},{""internalType"":""address"",""name"":""spender"",""type"":""address""}],""name"":""allowance"",""outputs"":[{""internalType"":""uint256"",""name"":""remaining"",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function""},{""inputs"":[{""internalType"":""address"",""name"":""account"",""type"":""address""}],""name"":""balanceOf"",""outputs"":[{""internalType"":""uint256"",""name"":""balance"",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function""},{""inputs"":[],""name"":""decimals"",""outputs"":[{""internalType"":""uint256"",""name"":"""",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function""},{""inputs"":[],""name"":""name"",""outputs"":[{""internalType"":""string"",""name"":"""",""type"":""string""}],""stateMutability"":""view"",""type"":""function""},{""inputs"":[],""name"":""symbol"",""outputs"":[{""internalType"":""string"",""name"":"""",""type"":""string""}],""stateMutability"":""view"",""type"":""function""},{""inputs"":[],""name"":""totalSupply"",""outputs"":[{""internalType"":""uint256"",""name"":"""",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function""}]";
        static async Task Main(string[] args)
        {
            var account = new Account("77d6fa458a588724ee56e756f08ba385a1c2c7f5a08ec3ce3d83c5a24046a7ea");
            var web3 = new Web3(account, "https://holesky.infura.io/v3/af0ede144313494094e840cec54e1aed");

            var contract = web3.Eth.GetContract(abi, contractAddress);

            var balanceOfFunction = contract.GetFunction("balanceOf");

            var balance = await balanceOfFunction.CallAsync<BigInteger>("0x0102bB2a98065e793E9AE6aec95E81ae0aF12605");

            var decimals = 6;

            var tokenAmount = Web3.Convert.FromWei(balance, decimals);

            Console.WriteLine(tokenAmount);

            var receiverAddress = "0xB59aB3f970befDd1e1B009376083FB69c339D5ba";

            var transferHandler = web3.Eth.GetContractTransactionHandler<TransferFunction>();
            var transfer = new TransferFunction()
            {
                To = receiverAddress,
                TokenAmount = 100000
            };
            var transactionReceipt = await transferHandler.SendRequestAndWaitForReceiptAsync(contractAddress, transfer);
            var balanceAfterTransfer = await balanceOfFunction.CallAsync<BigInteger>(receiverAddress);

            var tokens = Web3.Convert.FromWei(balanceAfterTransfer, decimals);

            Console.WriteLine(tokens);
        }
    }
}