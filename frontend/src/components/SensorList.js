import React, { useEffect, useState } from "react";
import {
  getSensors,
  getRecentMeasurement,
  getSensorDetails,
} from "../services/sensorService";
import FilterForm from "./FilterForm";
import "./styles/SensorList.css";

const SensorList = () => {
  const [sensors, setSensors] = useState([]);
  const [filter, setFilter] = useState({
    startDate: "",
    endDate: "",
    sensorType: "",
    sensorInstance: "",
  });
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

  useEffect(() => {
    const fetchData = async () => {
      try {
        const sensorData = await getSensors();
        if (!sensorData) {
          throw new Error("Sensor data is undefined");
        }
        const sensorsWithData = await Promise.all(
          sensorData.map(async (sensor) => {
            const recentData = await getRecentMeasurement(sensor.id);
            const sensorDetails = await getSensorDetails(sensor.id);
            const average = calculateAverage(sensorDetails.slice(-100));
            return { ...sensor, recentData, average };
          }),
        );
        setSensors(sensorsWithData);
      } catch (error) {
        console.error("Error fetching sensor data:", error);
      }
    };

    fetchData();
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
            <th>Average (Last 100)</th>
            <th>Sensor Details</th>
          </tr>
        </thead>
        <tbody>
          {filteredSensors.map((sensor) => (
            <tr key={sensor.id}>
              <td>{sensor.name}</td>
              <td>
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

export default SensorList;
