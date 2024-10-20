import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { Line } from "react-chartjs-2"; // Import Line chart from react-chartjs-2
import "chart.js/auto"; // Importing the auto version of chart.js
import "./styles/SensorDetails.css";
import { getSensorDetails, getSensorInformation } from "../services/sensorService";

const SensorDetails = () => {
  const { id: sensorId } = useParams();
  const [sensorData, setSensorData] = useState([]);
  const [sensorInformation, setSensorInformation] = useState(null);
  const [sensorUnit, setSensorUnit] = useState("");

  useEffect(() => {
    const fetchData = async () => {
      try {
        const sensorInformation = await getSensorInformation(sensorId);
        if (!sensorInformation) {
          throw new Error("Sensor information is undefined");
        }

        const sensorDetails = await getSensorDetails(sensorId); // Assume it's an array of values
        if (!sensorDetails || !Array.isArray(sensorDetails)) {
          throw new Error("Sensor data is undefined or not an array");
        }

        let unit = "";
        switch (sensorInformation.type) {
          case "Temperature":
            unit = "°C";
            break;
          case "Pressure":
            unit = "hPa";
            break;
          case "Humidity":
            unit = "g/m³";
            break;
          case "WindSpeed":
            unit = "km/h";
            break;
          default:
            unit = "";
        }

        setSensorUnit(unit);
        setSensorInformation(sensorInformation);
        setSensorData(sensorDetails); // Set array data here
      } catch (error) {
        console.error("Error fetching sensor data:", error);
      }
    };

    fetchData();
  }, [sensorId]);

  // Chart configuration
  const chartData = {
    labels: sensorData.map((d) => new Date(d.timestamp).toLocaleString()),
    datasets: [
      {
        label: `Sensor Data (${sensorUnit})`,
        data: sensorData.map((data) => data.value),
        fill: false,
        backgroundColor: "rgba(75,192,192,0.6)",
        borderColor: "rgba(75,192,192,1)",
        borderWidth: 2,
        tension: 0.1, // Smoother curve for the line
      },
    ],
  };

  const chartOptions = {
    responsive: true,
    scales: {
      y: {
        title: {
          display: true,
          text: `Value (${sensorUnit})`, // Y-axis label with the unit
        },
      },
      x: {
        title: {
          display: true,
          text: "Timestamp", // X-axis label
        },
      },
    },
  };

  return (
    <div className="sensor-container">
      <h1>Sensor Data Details</h1>
      {sensorData.length > 0 && sensorInformation ? (
        <div className="sensor-details">
          <h2>{sensorInformation.name}</h2>
          <p>
            <strong>Type:</strong> {sensorInformation.type}
          </p>
          <Line data={chartData} options={chartOptions} /> {/* Render the chart */}
        </div>
      ) : (
        <div className="loading">Loading sensor data...</div>
      )}
    </div>
  );
};

export default SensorDetails;
