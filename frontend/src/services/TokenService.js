const API_BASE_URL = process.env.BACKEND_URL || "http://localhost:8000/api";

const delay = (ms) => new Promise((resolve) => setTimeout(resolve, ms));

export const getTokenBalance = async (
  sensorAddress,
  retries = 5,
  delayMs = 1000,
) => {
  let attempt = 0;

  while (attempt < retries) {
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
      attempt++;
      console.error(
        `Error fetching token balance (Attempt ${attempt}):`,
        error,
      );

      if (attempt >= retries) {
        console.error("Max retries reached. Returning empty array.");
        return [];
      }

      await delay(delayMs);
    }
  }
};
