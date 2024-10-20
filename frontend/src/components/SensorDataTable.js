import React, { useEffect, useState } from "react";
import { getSensors, getData } from "../services/sensorService";
import "./styles/SensorDataTable.css";

const SensorDataTable = () => {
  const [allData, setAllData] = useState([]);
  const [filteredData, setFilteredData] = useState([]);
  const [currentPage, setCurrentPage] = useState(1);
  const [filters, setFilters] = useState({
    sensorName: "",
    sensorType: "",
    startDate: "",
    endDate: "",
  });
  const [sortConfig, setSortConfig] = useState({ key: "", direction: "asc" });

  const itemsPerPage = 16;

  const fetchAllData = async () => {
    try {
      const sensors = await getSensors();
      if (!sensors) throw new Error("Sensor data is undefined");

      const data = await getData();
      if (!data) throw new Error("Data is undefined");

      const mergedData = data.map((item) => {
        const sensor = sensors.find((sensor) => sensor.id === item.sensorId);
        return {
          ...item,
          sensorName: sensor ? sensor.name : "Unknown",
          sensorType: sensor ? sensor.type : "Unknown",
        };
      });

      setAllData(mergedData);
      setFilteredData(mergedData);
    } catch (error) {
      console.error("Error fetching data:", error);
    }
  };

  useEffect(() => {
    fetchAllData();
  }, []);

  const filterData = () => {
    const { sensorName, sensorType, startDate, endDate } = filters;
    let filtered = allData;

    if (sensorName) {
      filtered = filtered.filter((data) =>
        data.sensorName.toLowerCase().includes(sensorName.toLowerCase()),
      );
    }
    if (sensorType) {
      filtered = filtered.filter((data) =>
        data.sensorType.toLowerCase().includes(sensorType.toLowerCase()),
      );
    }
    if (startDate) {
      filtered = filtered.filter(
        (data) => new Date(data.timestamp) >= new Date(startDate),
      );
    }
    if (endDate) {
      filtered = filtered.filter(
        (data) => new Date(data.timestamp) <= new Date(endDate),
      );
    }

    setFilteredData(filtered);
    setCurrentPage(1);
  };

  const handleFilterChange = (e) => {
    setFilters({
      ...filters,
      [e.target.name]: e.target.value,
    });
  };

  useEffect(() => {
    filterData();
  }, [filters, allData]);

  const sortData = (key) => {
    let direction = "asc";
    if (sortConfig.key === key && sortConfig.direction === "asc") {
      direction = "desc";
    }
    setSortConfig({ key, direction });

    const sortedData = [...filteredData].sort((a, b) => {
      if (key === "value") {
        return direction === "asc" ? a.value - b.value : b.value - a.value;
      } else if (key === "timestamp") {
        return direction === "asc"
          ? new Date(a.timestamp) - new Date(b.timestamp)
          : new Date(b.timestamp) - new Date(a.timestamp);
      } else {
        const aValue = a[key] ? a[key].toString().toLowerCase() : "";
        const bValue = b[key] ? b[key].toString().toLowerCase() : "";
        if (aValue < bValue) return direction === "asc" ? -1 : 1;
        if (aValue > bValue) return direction === "asc" ? 1 : -1;
        return 0;
      }
    });

    setFilteredData(sortedData);
  };

  const handlePageChange = (newPage) => {
    setCurrentPage(newPage);
  };

  const startIndex = (currentPage - 1) * itemsPerPage;
  const paginatedData = filteredData.slice(
    startIndex,
    startIndex + itemsPerPage,
  );
  const totalPages = Math.ceil(filteredData.length / itemsPerPage);

  return (
    <div className="sensor-data-table">
      <h1>All Sensor Data</h1>

      <div className="filters">
        <input
          type="text"
          name="sensorName"
          placeholder="Filter by Sensor Name"
          value={filters.sensorName}
          onChange={handleFilterChange}
        />
        <input
          type="text"
          name="sensorType"
          placeholder="Filter by Sensor Type"
          value={filters.sensorType}
          onChange={handleFilterChange}
        />
        <input
          type="datetime-local"
          name="startDate"
          value={filters.startDate}
          onChange={handleFilterChange}
        />
        <input
          type="datetime-local"
          name="endDate"
          value={filters.endDate}
          onChange={handleFilterChange}
        />
      </div>

      <table className="sensor-table">
        <thead>
          <tr>
            <th onClick={() => sortData("sensorName")}>
              Sensor Name{" "}
              {sortConfig.key === "sensorName"
                ? sortConfig.direction === "asc"
                  ? "▲"
                  : "▼"
                : ""}
            </th>
            <th onClick={() => sortData("sensorType")}>
              Sensor Type{" "}
              {sortConfig.key === "sensorType"
                ? sortConfig.direction === "asc"
                  ? "▲"
                  : "▼"
                : ""}
            </th>
            <th onClick={() => sortData("value")}>
              Value{" "}
              {sortConfig.key === "value"
                ? sortConfig.direction === "asc"
                  ? "▲"
                  : "▼"
                : ""}
            </th>
            <th onClick={() => sortData("timestamp")}>
              Timestamp{" "}
              {sortConfig.key === "timestamp"
                ? sortConfig.direction === "asc"
                  ? "▲"
                  : "▼"
                : ""}
            </th>
          </tr>
        </thead>
        <tbody>
          {paginatedData.length > 0 ? (
            paginatedData.map((data) => (
              <tr key={data.id}>
                <td>{data.sensorName}</td>
                <td>{data.sensorType}</td>
                <td>{data.value.toFixed(2)}</td>
                <td>{new Date(data.timestamp).toLocaleString()}</td>
              </tr>
            ))
          ) : (
            <tr>
              <td colSpan="4">No data available</td>
            </tr>
          )}
        </tbody>
      </table>

      <div className="pagination">
        {Array.from({ length: totalPages }, (_, index) => (
          <button
            key={index}
            onClick={() => handlePageChange(index + 1)}
            disabled={currentPage === index + 1}
          >
            {index + 1}
          </button>
        ))}
      </div>
    </div>
  );
};

export default SensorDataTable;
