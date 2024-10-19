# WeatherStation

## To start the app:
1. Clone the repository
2. Run `docker compose up --build` in the root directory
3. Open `http://localhost:8000/api/sensors` in your browser
4. Go to the GeneratorScript directory
5. Run `python3 -m venv venv`
6. Run `source venv/bin/activate`
7. Run `pip install -r requirements.txt`
8. Run `python3 main.py`
9. Refresh `http://localhost:8000/api/sensors` in your browser to see the results
10. To stop the app, run `docker compose down -v` in the root directory. It will remove the database volume as well.


## API Endpoints

### Data

- **GET `/api/data`**
    - Get all sensor data records with optional filtering.
    - Query Parameters:
        - `sensorType` (optional): The type of sensor (e.g., `Temperature`, `Humidity`, `Pressure`, `WindSpeed`).
        - `sensorId` (optional): The ID of the sensor.
        - `startDate` (optional): The start date for filtering data (format: `YYYY-MM-DDTHH:MM:SS`).
        - `endDate` (optional): The end date for filtering data (format: `YYYY-MM-DDTHH:MM:SS`).
        - `limit` (optional): The maximum number of records to return.
        - `sortBy` (optional): The field to sort by (e.g., `timestamp`, `value`).
        - `sortOrder` (optional): The sort order (e.g., `asc`, `desc`).
        - `export` (optional): Export the data to a CSV or JSON file (e.g., `csv`, `json`).

### Sensor

- **GET `/api/sensors`**
   - Get a list of all sensors.

- **GET `/api/sensors/{id}`**
   - Get information about a specific sensor by its ID.

- **POST `/api/sensors`**
   - Create a new sensor.

- **PATCH `/api/sensors/{id}`**
   - Update a specific sensor by its ID.

- **DELETE `/api/sensors/{id}`**
   - Delete a specific sensor by its ID.

### Token

- **GET `/api/sensors/{id}/tokens`**
   - Get the token balance for a specific sensor by its ID.

### Swagger

- **GET `/api/swagger`**
   - Access the Swagger documentation for the API.

# Blockchain Module Setup

## Metamask Configuration
1. **Set up Holesky testnet**  
   In Metamask, add the Holesky testnet by providing the necessary network details.

2. **Receive Test ETH**  
   Use a Holesky testnet faucet to receive test ETH, for example, from the [Ethereum Holešky Faucet](https://cloud.google.com/application/web3/faucet/ethereum/holesky).

3. **Import Sensor Coins**  
   To view Sensor Coins balance, import the tokens into Metamask by providing the contract address.

## ERC-20 Smart Contract Deployment
1. **Program Smart Contract**  
   Use Solidity to program an ERC-20 smart contract.

2. **Compile and Deploy**  
   Use [Remix IDE](https://remix.ethereum.org/) to compile the smart contract and deploy it to the Holesky testnet.

## Blockchain Network Access
1. **Create Infura Account**  
   Sign up at Infura and create a project to get your API key for blockchain access.

## Monitoring
- Use the [Etherscan Holesky Explorer](https://holesky.etherscan.io) to monitor transactions related to your deployed contract.
- Transaction related to this project can be found [here](https://holesky.etherscan.io/token/0xe99ddc1405e2a5c2c4d57642ea742706a9ddb750).


