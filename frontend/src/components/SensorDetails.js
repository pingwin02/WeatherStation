import React, { useEffect, useState } from "react";
import { Line } from "react-chartjs-2";
import { getSensorDetails } from "../services/sensorService";
import { useParams } from "react-router-dom";
import { Chart, registerables } from "chart.js";

Chart.register(...registerables);

const SensorDetails = () => {
  const { id: sensorId } = useParams();
  const [sensorData, setSensorData] = useState([]);
  const [average, setAverage] = useState(0);

  const getAverage = (data) => {
    const total = data.reduce((sum, item) => sum + item.value, 0);
    return total / data.length;
  };

  useEffect(() => {
    const fetchData = async () => {
      const data = await getSensorDetails(sensorId);
      setSensorData(data.slice(-100));
      setAverage(getAverage(data.slice(-100)));
    };

    fetchData();
  }, [sensorId]);

  const data = {
    labels: sensorData.map((d) => new Date(d.timestamp).toLocaleString()),
    datasets: [
      {
        label: "Sensor Value",
        data: sensorData.map((d) => d.value),
        fill: false,
        borderColor: "rgba(75,192,192,1)",
      },
    ],
  };

  return (
    <div>
      <h1>Sensor Details</h1>
      <Line data={data} />
    </div>
  );
};

export default SensorDetails;
