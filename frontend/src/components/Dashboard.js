import React, { useEffect, useState } from "react";
import {
  getSensors,
  getRecentMeasurement,
  getSensorData,
  initWebSocket,
} from "../services/SensorService";
import "./styles/Dashboard.css";

const HISTORY_LIMIT = 100;

const Dashboard = () => {
  const [sensors, setSensors] = useState([]);
  const [collapsed, setCollapsed] = useState({
    Temperature: true,
    WindSpeed: true,
    Humidity: true,
    Pressure: true,
  });

  const calculateAverage = (data) => {
    if (!data || data.length === 0) return 0;
    const total = data.reduce((sum, item) => sum + item.value, 0);
    return total / data.length;
  };

  const fetchData = async () => {
    try {
      const sensorData = await getSensors();
      if (!sensorData) {
        throw new Error("Sensor data is undefined");
      }
      const sensorsWithData = await Promise.all(
        sensorData.map(async (sensor) => {
          const recentData = await getRecentMeasurement(sensor.id);
          const allSensorData = await getSensorData(sensor.id);
          const historicalData = allSensorData.slice(-HISTORY_LIMIT);
          const average = calculateAverage(historicalData);
          return { ...sensor, recentData, average, historicalData };
        }),
      );
      setSensors(sensorsWithData);
    } catch (error) {
      console.error("Error fetching sensor data:", error);
    }
  };

  useEffect(() => {
    fetchData();

    const socket = initWebSocket((incomingData) => {
      setSensors((prevSensors) => {
        return prevSensors.map((sensor) => {
          if (sensor.id === incomingData.sensorId) {
            const updatedSensor = { ...sensor };

            const historicalData = updatedSensor.historicalData || [];
            if (historicalData.length >= HISTORY_LIMIT) {
              historicalData.shift();
            }
            updatedSensor.historicalData = historicalData;
            updatedSensor.recentData = incomingData;

            let lastData = incomingData;
            if (historicalData.length > 0) {
              lastData = historicalData[historicalData.length - 1];
            }

            updatedSensor.trend =
              lastData.value < incomingData.value
                ? "value-up"
                : lastData.value > incomingData.value
                  ? "value-down"
                  : "equal";

            historicalData.push(incomingData);
            updatedSensor.average = calculateAverage(historicalData);
            return updatedSensor;
          }
          return sensor;
        });
      });
    });

    return () => {
      if (socket && socket.readyState === WebSocket.OPEN) {
        socket.close();
      }
    };
  }, []);

  const toggleCollapse = (type) => {
    setCollapsed((prevState) => ({
      ...prevState,
      [type]: !prevState[type],
    }));
  };

  const filterByType = (type) => {
    return sensors.filter((sensor) => sensor.type === type);
  };

  const hasData = (sensorType) => {
    const filteredSensors = filterByType(sensorType);
    return filteredSensors.length > 0;
  };

  const renderSensorTable = (sensorType) => {
    const filteredSensors = filterByType(sensorType);
    return (
      <table className="sensor-table">
        <thead>
          <tr>
            <th>Sensor Name</th>
            <th>Last Value</th>
            <th>Average (Last {HISTORY_LIMIT})</th>
            <th>Sensor Details</th>
          </tr>
        </thead>
        <tbody>
          {filteredSensors.map((sensor) => (
            <tr key={sensor.id}>
              <td>{sensor.name}</td>
              <td className={sensor.trend}>
                {sensor.recentData
                  ? sensor.recentData.value
                  : "No data available"}
              </td>
              <td>
                {sensor.recentData
                  ? sensor.average.toFixed(2)
                  : "No data available"}
              </td>
              <td>
                <a href={`/sensors/${sensor.id}`}>View Details</a>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    );
  };

  return (
    <div className="sensor-list-container">
      <h1>Dashboard</h1>
      <div className="sensor-section">
        <h2
          onClick={() => toggleCollapse("Temperature")}
          className="collapsible-header"
        >
          Temperature {collapsed.Temperature ? "▼" : "▶"}
        </h2>
        {collapsed.Temperature &&
          (hasData("Temperature") ? (
            renderSensorTable("Temperature")
          ) : (
            <p>No data available</p>
          ))}
      </div>
      <div className="sensor-section">
        <h2
          onClick={() => toggleCollapse("WindSpeed")}
          className="collapsible-header"
        >
          WindSpeed {collapsed.WindSpeed ? "▼" : "▶"}
        </h2>
        {collapsed.WindSpeed &&
          (hasData("WindSpeed") ? (
            renderSensorTable("WindSpeed")
          ) : (
            <p>No data available</p>
          ))}
      </div>
      <div className="sensor-section">
        <h2
          onClick={() => toggleCollapse("Humidity")}
          className="collapsible-header"
        >
          Humidity {collapsed.Humidity ? "▼" : "▶"}
        </h2>
        {collapsed.Humidity &&
          (hasData("Humidity") ? (
            renderSensorTable("Humidity")
          ) : (
            <p>No data available</p>
          ))}
      </div>
      <div className="sensor-section">
        <h2
          onClick={() => toggleCollapse("Pressure")}
          className="collapsible-header"
        >
          Pressure {collapsed.Pressure ? "▼" : "▶"}
        </h2>
        {collapsed.Pressure &&
          (hasData("Pressure") ? (
            renderSensorTable("Pressure")
          ) : (
            <p>No data available</p>
          ))}
      </div>
    </div>
  );
};

export default Dashboard;
