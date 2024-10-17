import React, { useEffect, useState } from 'react';
import { getSensors, getRecentMeasurement } from '../services/sensorService';

const SensorList = () => {
    const [sensors, setSensors] = useState([]);

    useEffect(() => {
        const fetchData = async () => {
            try {
                const sensorData = await getSensors();
                console.log(sensorData); // Log the response
                if (!sensorData) {
                    throw new Error("Sensor data is undefined");
                }
                const sensorsWithData = await Promise.all(sensorData.map(async sensor => {
                    const recentData = await getRecentMeasurement(sensor.id);
                    return { ...sensor, recentData };
                }));
                setSensors(sensorsWithData);
            } catch (error) {
                console.error("Error fetching sensor data:", error);
                // Optionally, you could set an error state here to display an error message to the user.
            }
        };

        fetchData();
    }, []);

    return (
        <div>
            <h1>Sensors</h1>
            <ul>
                {sensors.map(sensor => (
                    <li key={sensor.id}>
                        {sensor.name}: {sensor.recentData ? sensor.recentData.value : 'No data available'}
                    </li>
                ))}
            </ul>
        </div>
    );
};

export default SensorList;
