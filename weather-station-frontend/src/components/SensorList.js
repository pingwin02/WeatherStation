import React, { useEffect, useState } from 'react';
import { getSensors, getRecentMeasurement } from '../services/sensorService';
import FilterForm from './FilterForm';

const SensorList = () => {
    const [sensors, setSensors] = useState([]);
    const [filter, setFilter] = useState({
        startDate: '',
        endDate: '',
        sensorType: '',
        sensorInstance: ''
    });
    const [collapsed, setCollapsed] = useState({
        Temperature: true,
        WindSpeed: true,
        Humidity: true,
        Pressure: true
    });


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
                    const average = 1;
                    return { ...sensor, recentData, average };
                }));
                setSensors(sensorsWithData);
            } catch (error) {
                console.error("Error fetching sensor data:", error);
                // Optionally, you could set an error state here to display an error message to the user.
            }
        };

        fetchData();
    }, []);

    const toggleCollapse = (type) => {
        setCollapsed(prevState => ({
            ...prevState,
            [type]: !prevState[type]
        }));
    };

    // Filter sensors by type
    const filterByType = (type) => {
        return sensors.filter(sensor => sensor.type === type);
    };

    const renderSensorTable = (sensorType) => {
        const filteredSensors = filterByType(sensorType);
        return (
            <table>
                <thead>
                    <tr>
                        <th>Sensor Name</th>
                        <th>Last Value</th>
                        <th>Average (Last 100)</th>
                    </tr>
                </thead>
                <tbody>
                    {filteredSensors.map(sensor => (
                        <tr key={sensor.id}>
                            <td>{sensor.name}</td>
                            <td>{sensor.recentData ? sensor.recentData.value : 'No data available'}</td>
                            <td>{sensor.average}</td>
                        </tr>
                    ))}
                </tbody>
            </table>
        );
    };

    return (
        <div>
            <h1>Sensors</h1>
            <FilterForm filter={filter} setFilter={setFilter} />
            <div>
                <h2 onClick={() => toggleCollapse('Temperature')}>
                    Temperature {collapsed.Temperature ? '▼' : '▲'}
                </h2>
                {collapsed.Temperature && renderSensorTable('Temperature')}
            </div>
            <div>
                <h2 onClick={() => toggleCollapse('WindSpeed')}>
                    WindSpeed {collapsed.Wind ? '▼' : '▲'}
                </h2>
                {collapsed.WindSpeed && renderSensorTable('WindSpeed')}
            </div>
            <div>
                <h2 onClick={() => toggleCollapse('Humidity')}>
                    Humidity {collapsed.Other1 ? '▼' : '▲'}
                </h2>
                {collapsed.Humidity && renderSensorTable('Humidity')}
            </div>
            <div>
                <h2 onClick={() => toggleCollapse('Pressure')}>
                    Pressure {collapsed.Other2 ? '▼' : '▲'}
                </h2>
                {collapsed.Pressure && renderSensorTable('Pressure')}
            </div>
        </div>
    );
};

export default SensorList;
