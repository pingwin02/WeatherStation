import axios from 'axios';

const API_URL = '/api/tokens';

export const getTokenBalance = async (sensorAddress) => {
    const response = await axios.get(`${API_URL}/balance/${sensorAddress}`);
    return response.data;
};
