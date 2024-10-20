import React, { useEffect, useState } from "react";
import { getTokenBalance } from "../services/tokenService";
import { getSensors } from "../services/sensorService";
import "./styles/TokenBalance.css";

const TokenBalance = () => {
  const [tokenBalances, setTokenBalances] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchData = async () => {
      try {
        const sensorData = await getSensors();
        if (!sensorData) {
          throw new Error("Sensor data is undefined");
        }
        const sensorsWithData = await Promise.all(
          sensorData.map(async (sensor) => {
            const tokenBalance = await getTokenBalance(sensor.id);
            return { ...sensor, balance: tokenBalance.Balance };
          })
        );
        setTokenBalances(sensorsWithData);
      } catch (error) {
        console.error("Error fetching sensor data:", error);
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, []);

  return (
    <div className="table-container">
      <h1>Token Balances</h1>
      {loading ? (
        <div className="spinner-container">
          <div className="spinner"></div>
          <p>Loading...</p>
        </div>
      ) : (
        <table className="styled-table">
          <thead>
            <tr>
              <th>Sensor Id</th>
              <th>Sensor Name</th>
              <th>Sensor Wallet Address</th>
              <th>Token Balance</th>
            </tr>
          </thead>
          <tbody>
            {tokenBalances.map((sensor) => (
              <tr key={sensor.id}>
                <td>{sensor.id}</td>
                <td>{sensor.name}</td>
                <td>{sensor.wallet_address}</td>
                <td>{sensor.balance}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
};

export default TokenBalance;
