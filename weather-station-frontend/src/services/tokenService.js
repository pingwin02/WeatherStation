import axios from 'axios';

export const getTokenBalance = async (sensorAddress) => {
    try {
        const response = await fetch(`http://localhost:8000/api/sensors/${sensorAddress}/tokens`);
        if (!response.ok) {
            throw new Error(`Error: ${response.status} ${response.statusText}`);
        }
        const data = await response.json();
        return data;
    } catch (error) {
        console.error("Error fetching token balance:", error);
        return [];
    }
};
