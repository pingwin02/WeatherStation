import axios from 'axios';

const API_BASE_URL = "http://localhost:8000";
export const getSensors = async () => {
    try {
        const response = await fetch('http://localhost:8000/api/sensors');
        if (!response.ok) {
            throw new Error(`HTTP error! Status: ${response.status}`);
        }
        const data = await response.json();
        return data; // Ensure this returns the expected array of sensors
    } catch (error) {
        console.error("Error fetching sensors:", error);
        return []; // Return an empty array in case of error to prevent breaking the UI
    }
};
export const getData = async () => {
    try {
        const response = await fetch('http://localhost:8000/api/data');
        if (!response.ok) {
            throw new Error(`HTTP error! Status: ${response.status}`);
        }
        const data = await response.json();
        return data; // Ensure this returns the expected array of sensors
    } catch (error) {
        console.error("Error fetching sensors:", error);
        return []; // Return an empty array in case of error to prevent breaking the UI
    }
};
export const getRecentMeasurement = async (sensorId) => {
    try {
        const response = await fetch(`http://localhost:8000/api/sensors/${sensorId}/data/recent`);
        if (!response.ok) {
            throw new Error(`HTTP error! Status: ${response.status}`);
        }
        const data = await response.json();
        return data; // Ensure this returns the expected recent measurement object
    }
    catch (error) {
        console.error("Error fetching recent measurement:", error);
        return null; // Return null in case of error to prevent breaking the UI
    }
};

export const getSensorDetails = async (sensorId) => {
    try {
        const response = await fetch(`http://localhost:8000/api/sensors/${sensorId}/data`);
        if (!response.ok) {
            throw new Error(`HTTP error! Status: ${response.status}`);
        }
        const data = await response.json();
        return data; // Ensure this returns the expected array of sensor data
        
    }
    catch (error) {
        console.error("Error fetching sensor details:", error);
        return []; // Return an empty array in case of error to prevent breaking the UI
    }
};
