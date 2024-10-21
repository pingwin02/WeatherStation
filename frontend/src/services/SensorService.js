const API_BASE_URL = process.env.BACKEND_URL || "http://localhost:8000/api";
const WEB_SOCKET_URL = process.env.WEB_SOCKET_URL || "http://localhost:8000/ws";

export const getSensors = async () => {
  try {
    const response = await fetch(`${API_BASE_URL}/sensors`);
    if (!response.ok) {
      throw new Error(`HTTP error! Status: ${response.status}`);
    }
    const data = await response.json();
    return data;
  } catch (error) {
    console.error("Error fetching sensors:", error);
    return [];
  }
};

export const getData = async () => {
  try {
    const response = await fetch(`${API_BASE_URL}/data`);
    if (!response.ok) {
      throw new Error(`HTTP error! Status: ${response.status}`);
    }
    const data = await response.json();
    return data;
  } catch (error) {
    console.error("Error fetching data:", error);
    return [];
  }
};

export const getRecentMeasurement = async (sensorId) => {
  try {
    const response = await fetch(
      `${API_BASE_URL}/data?sensorId=${sensorId}&sortBy=timestamp&sortOrder=desc&limit=1`,
    );
    if (!response.ok) {
      throw new Error(`HTTP error! Status: ${response.status}`);
    }
    const [data] = await response.json();
    return data;
  } catch (error) {
    console.error("Error fetching recent measurement:", error);
    return null;
  }
};

export const getSensorData = async (sensorId) => {
  try {
    const response = await fetch(`${API_BASE_URL}/data?sensorId=${sensorId}`);
    if (!response.ok) {
      throw new Error(`HTTP error! Status: ${response.status}`);
    }
    const data = await response.json();
    return data;
  } catch (error) {
    console.error("Error fetching sensor data:", error);
    return [];
  }
};

export const initWebSocket = (onMessageReceived) => {
  const socket = new WebSocket(WEB_SOCKET_URL);

  socket.onmessage = (event) => {
    const newData = JSON.parse(event.data);
    onMessageReceived(newData);
  };

  return socket;
};

export const getSensorInformation = async (sensorId) => {
  try {
    const response = await fetch(`${API_BASE_URL}/sensors/${sensorId}`);
    if (!response.ok) {
      throw new Error(`HTTP error! Status: ${response.status}`);
    }
    const data = await response.json();
    return data;
  } catch (error) {
    console.error("Error fetching sensor information:", error);
    return [];
  }
};

export const getDataFiltered = async (queryString) => {
  try {
    const response = await fetch(`${API_BASE_URL}/data${queryString}`);
    if (!response.ok) {
      throw new Error(`HTTP error! Status: ${response.status}`);
    }
    return response;
  } catch (error) {
    console.error("Error fetching filtered data:", error);
    return [];
  }
};
