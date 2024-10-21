const API_BASE_URL = process.env.BACKEND_URL || "http://localhost:8000/api";
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
    console.error("Error fetching sensors:", error);
    return [];
  }
};
export const getRecentMeasurement = async (sensorId) => {
  try {
    const response = await fetch(
      `${API_BASE_URL}/data?sensorId=${sensorId}&sortBy=timestamp&sortOrder=desc&limit=1`
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

export const getSensorDetails = async (sensorId) => {
  try {
    const response = await fetch(`${API_BASE_URL}/data?sensorId=${sensorId}`);
    if (!response.ok) {
      throw new Error(`HTTP error! Status: ${response.status}`);
    }
    const data = await response.json();
    return data;
  } catch (error) {
    console.error("Error fetching sensor details:", error);
    return [];
  }
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
    console.error("Error fetching sensor details:", error);
    return [];
  }
};

export const getFilteredDataTable = async (queryString) => {
  try {
    const response = await fetch(`${API_BASE_URL}/data${queryString}`);
    if (!response.ok) {
      throw new Error(`HTTP error! Status: ${response.status}`);
    }
    const data = await response.json();
    return data;
  } catch (error) {
    console.error("Error fetching sensor details:", error);
    return [];
  }
};


