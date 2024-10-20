const API_BASE_URL = process.env.BACKEND_URL || "http://localhost:8000/api";
export const getTokenBalance = async (sensorAddress) => {
  try {
    const response = await fetch(
      `${API_BASE_URL}/sensors/${sensorAddress}/tokens`,
    );
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
