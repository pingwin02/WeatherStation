# WeatherStation

## To start the app:
1. Clone the repository
2. Run `docker compose up --build` in the root directory
3. Open `http://localhost:8000/api/sensors` in your browser
4. Go to the GeneratorScript directory
5. Run `python3 -m venv venv`
6. Run `source venv/bin/activate`
7. Run `pip install -r requirements.txt`
8. Run `python3 sensors_data_generator.py`
9. Refresh `http://localhost:8000/api/sensors` in your browser to see the results
10. To stop the app, run `docker compose down -v` in the root directory. It will remove the database volume as well.


## Endpoints:

### Sensors:

- GET `/api/sensors` - get a list of all sensors
- GET `/api/sensors/{id}` - get information about a specific sensor by its ID
- POST `/api/sensors` - create a new sensor
- PATCH `/api/sensors/{id}` - update a specific sensor by its ID
- DELETE `/api/sensors/{id}` - delete a specific sensor by its ID

### Data:

- GET `/api/data` - get all data
- GET `/api/sensors/{id}/data` - get all data for a specific sensor by its ID
- GET `/api/sensors/{id}/data/recent` - get the most recent data for a specific sensor by its ID

### Tokens:

- GET `/api/tokens/{id}` - get sensor balance by its ID

### Swagger:

- `/api/swagger` - Swagger documentation

## Blockchain module:

### Metamask 
1. Set up Holesky testnet
2. Receive test ETH, using faucet
3. To see Sensor Coins balance, import tokens by providing contract address

### ERC-20 Smart Contract
Using [Remix](https://remix.ethereum.org/#lang=en&optimize=false&runs=200&evmVersion=null&version=soljson-v0.8.26+commit.8a97fa7a.js)
1. Program smart contract in solidity
2. Compile it and deploy to testnet

### Access to blockchain network
1. Create account at [Infura](https://app.infura.io/)
2. Receive an API key 

### Monitoring
Go to [Ethrescan](https://holesky.etherscan.io/token/0xe99ddc1405e2a5c2c4d57642ea742706a9ddb750) to observe transactions


