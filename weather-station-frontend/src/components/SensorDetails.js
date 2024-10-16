import React, { useEffect, useState } from 'react';
import { Line } from 'react-chartjs-2';
import { getSensorDetails } from '../services/sensorService';
import { useParams } from 'react-router-dom';
import { Chart, registerables } from 'chart.js'; // Import Chart and registerables

Chart.register(...registerables);

const SensorDetails = () => {
    const { id: sensorId } = useParams();
    const [sensorData, setSensorData] = useState([]);

    useEffect(() => {
        const fetchData = async () => {
            const data = await getSensorDetails(sensorId);
            setSensorData(data.slice(-100));
        };

        fetchData();
    }, [sensorId]);

    const data = {
        labels: sensorData.map(d => new Date(d.timestamp).toLocaleString()), // Use 'timestamp' instead of 'createdAt'
        datasets: [
            {
                label: 'Sensor Value',
                data: sensorData.map(d => d.value), // Ensure you're using the correct property name
                fill: false,
                borderColor: 'rgba(75,192,192,1)',
            }
        ]
    };

    return (
        <div>
            <h1>Sensor Details</h1>
            <Line data={data} />
        </div>
    );
};

export default SensorDetails;
