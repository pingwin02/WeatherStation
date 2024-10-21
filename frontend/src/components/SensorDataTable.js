import React, { useEffect, useState } from "react";
import {
  getSensors,
  getData,
  getDataFiltered,
} from "../services/SensorService";
import { Line } from "react-chartjs-2";
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
} from "chart.js";
import "./styles/SensorDataTable.css";

const chartColors = [
  "rgba(255, 99, 132, 1)",
  "rgba(54, 162, 235, 1)",
  "rgba(255, 206, 86, 1)",
  "rgba(75, 192, 192, 1)",
  "rgba(153, 102, 255, 1)",
  "rgba(255, 159, 64, 1)",
];

const ITEMS_PER_PAGE = 16;

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
);

const SensorDataTable = () => {
  const [filteredData, setFilteredData] = useState([]);
  const [currentPage, setCurrentPage] = useState(1);
  const [viewMode, setViewMode] = useState("table");
  const [filters, setFilters] = useState({
    sensorId: "",
    sensorName: "",
    sensorType: "",
    startDate: "",
    endDate: "",
    limit: "",
    sortBy: "timestamp",
    sortOrder: "asc",
  });

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

      setFilteredData(mergedData);
    } catch (error) {
      console.error("Error fetching data:", error);
    }
  };

  useEffect(() => {
    fetchAllData();
  }, []);

  const filterData = async (format) => {
    const {
      sensorId,
      sensorName,
      sensorType,
      startDate,
      endDate,
      limit,
      sortBy,
      sortOrder,
    } = filters;
    let params = [];

    const startDateAdjusted = startDate
      ? new Date(new Date(startDate).getTime()).toISOString()
      : "";
    const endDateAdjusted = endDate
      ? new Date(new Date(endDate).getTime()).toISOString()
      : "";

    if (sensorId) params.push(`sensorId=${sensorId}`);
    if (sensorName) params.push(`sensorName=${encodeURIComponent(sensorName)}`);
    if (sensorType) params.push(`sensorType=${sensorType}`);
    if (startDate) params.push(`startDate=${startDateAdjusted}`);
    if (endDate) params.push(`endDate=${endDateAdjusted}`);
    if (limit) params.push(`limit=${limit}`);
    if (sortBy) params.push(`sortBy=${sortBy}`);
    if (sortOrder) params.push(`sortOrder=${sortOrder}`);

    const queryString = params.length ? `?${params.join("&")}` : "";

    if (format === "json" || format === "csv") {
      const downloadQuery = `${queryString}&export=${format}`;
      return await getDataFiltered(downloadQuery);
    }

    const filteredDataTableResponse = await getDataFiltered(queryString);
    if (!filteredDataTableResponse) return;
    const filteredDataTable = await filteredDataTableResponse.json();
    const sensors = await getSensors();

    const mergedData = filteredDataTable.map((item) => {
      const sensor = sensors.find((sensor) => sensor.id === item.sensorId);
      return {
        ...item,
        sensorName: sensor ? sensor.name : "Unknown",
        sensorType: sensor ? sensor.type : "Unknown",
      };
    });

    setFilteredData(mergedData);
    setCurrentPage(1);
  };

  const options = {
    responsive: true,
    plugins: {
      tooltip: {
        callbacks: {
          label: function (tooltipItem) {
            let label = tooltipItem.dataset.label || "";
            if (label) {
              label += ": ";
            }
            label += `${tooltipItem.raw}`;
            return label;
          },
        },
      },
    },
    scales: {
      x: {
        title: {
          display: true,
          text: "Timestamp",
        },
      },
      y: {
        title: {
          display: true,
          text: "Value",
        },
      },
    },
  };

  const handlePageChange = (newPage) => {
    setCurrentPage(newPage);
  };

  const handleFilterChange = (e) => {
    const { name, value } = e.target;
    setFilters((prevFilters) => ({
      ...prevFilters,
      [name]: value,
    }));
  };

  useEffect(() => {
    const filterDataOnChange = async () => {
      await filterData();
    };

    filterDataOnChange();
  }, [filters]);

  const handleViewToggle = () => {
    setViewMode(viewMode === "table" ? "chart" : "table");
  };

  const handleDownload = async (format) => {
    const response = await filterData(format);
    if (response.ok) {
      const blob = await response.blob();

      const formattedDate = new Date()
        .toISOString()
        .replace(/[-:]/g, "")
        .replace("T", "_")
        .split(".")[0];
      const filename = `data_${formattedDate}.${format}`;

      const link = document.createElement("a");
      link.href = URL.createObjectURL(blob);
      link.download = filename;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      URL.revokeObjectURL(link.href);
    } else {
      console.error("Download failed:", response.statusText);
    }
  };

  const handleResetFilters = () => {
    setFilters({
      sensorId: "",
      sensorName: "",
      sensorType: "",
      startDate: "",
      endDate: "",
      limit: "",
      sortBy: "timestamp",
      sortOrder: "asc",
    });
  };

  const handleSort = (column) => {
    const sortOrder =
      filters.sortBy === column && filters.sortOrder === "asc" ? "desc" : "asc";
    setFilters((prevFilters) => ({
      ...prevFilters,
      sortBy: column,
      sortOrder: sortOrder,
    }));
  };

  const getUnit = (sensorType) => {
    switch (sensorType) {
      case "WindSpeed":
        return "km/h";
      case "Pressure":
        return "hPa";
      case "Temperature":
        return "°C";
      case "Humidity":
        return "g/m³";
      default:
        return "";
    }
  };

  const getAllChartData = () => {
    const groupedData = filteredData.reduce((acc, data) => {
      const sensorType = data.sensorType;

      if (!acc[sensorType]) {
        acc[sensorType] = {};
      }

      if (!acc[sensorType][data.sensorName]) {
        acc[sensorType][data.sensorName] = [];
      }

      acc[sensorType][data.sensorName].push({
        timestamp: new Date(data.timestamp).toLocaleString(),
        value: data.value,
      });

      return acc;
    }, {});

    const chartsData = Object.entries(groupedData).map(
      ([sensorType, sensors]) => {
        const datasets = Object.entries(sensors)
          .slice(0, 4)
          .map(([sensorName, readings], index) => {
            return {
              label: sensorName,
              sensorType: sensorType,
              data: readings.map((reading) => reading.value),
              borderColor: chartColors[index % chartColors.length],
              backgroundColor: chartColors[index % chartColors.length],
              fill: false,
              tension: 0.1,
            };
          });

        const labels = sensors[Object.keys(sensors)[0]].map(
          (reading) => reading.timestamp,
        );

        return {
          labels: [...new Set(labels)],
          datasets,
        };
      },
    );

    return chartsData;
  };

  const startIndex = (currentPage - 1) * ITEMS_PER_PAGE;
  const paginatedData = filteredData.slice(
    startIndex,
    startIndex + ITEMS_PER_PAGE,
  );
  const totalPages = Math.ceil(filteredData.length / ITEMS_PER_PAGE);

  const getPaginationRange = () => {
    const range = [];
    const maxVisiblePages = 1;

    range.push(1);

    if (currentPage > maxVisiblePages + 1) {
      range.push("...");
    }

    for (
      let i = Math.max(2, currentPage - maxVisiblePages);
      i <= Math.min(totalPages - 1, currentPage + maxVisiblePages);
      i++
    ) {
      range.push(i);
    }

    if (currentPage < totalPages - maxVisiblePages) {
      range.push("...");
    }

    if (totalPages > 1) {
      range.push(totalPages);
    }

    return range;
  };

  const allChartData = getAllChartData();
  const isSingleChart = allChartData.length === 1;

  return (
    <div className="sensor-data-table">
      <h1>All Sensor Data</h1>

      <div className="filters">
        <input
          type="text"
          name="sensorId"
          placeholder="Filter by Sensor ID"
          value={filters.sensorId}
          onChange={handleFilterChange}
        />
        <input
          type="text"
          name="sensorName"
          placeholder="Filter by Sensor Name"
          value={filters.sensorName}
          onChange={handleFilterChange}
        />
        <select
          name="sensorType"
          value={filters.sensorType}
          onChange={handleFilterChange}
        >
          <option value="">All Sensor Types</option>
          <option value="Temperature">Temperature</option>
          <option value="Humidity">Humidity</option>
          <option value="Pressure">Pressure</option>
          <option value="WindSpeed">WindSpeed</option>
        </select>
        <input
          type="datetime-local"
          name="startDate"
          placeholder="Start Date"
          value={filters.startDate}
          onChange={handleFilterChange}
        />
        <input
          type="datetime-local"
          name="endDate"
          placeholder="End Date"
          value={filters.endDate}
          onChange={handleFilterChange}
        />
        <input
          type="number"
          name="limit"
          placeholder="Limit"
          value={filters.limit}
          onChange={handleFilterChange}
          style={{ width: "50px" }}
        />
        <button onClick={handleResetFilters}>Reset Filters</button>
        <button onClick={handleViewToggle}>
          {viewMode === "table"
            ? "Switch to Chart View"
            : "Switch to Table View"}
        </button>
      </div>
      {viewMode === "table" && (
        <>
          <button onClick={() => handleDownload("json")}>Download JSON</button>
          <button onClick={() => handleDownload("csv")}>Download CSV</button>
        </>
      )}
      {viewMode === "table" ? (
        <>
          <table className="sensor-table">
            <thead>
              <tr>
                <th onClick={() => handleSort("sensorId")}>
                  Sensor ID{" "}
                  {filters.sortBy === "sensorId" &&
                    (filters.sortOrder === "asc" ? "▲" : "▼")}
                </th>
                <th onClick={() => handleSort("sensorName")}>
                  Sensor Name{" "}
                  {filters.sortBy === "sensorName" &&
                    (filters.sortOrder === "asc" ? "▲" : "▼")}
                </th>
                <th onClick={() => handleSort("sensorType")}>
                  Sensor Type{" "}
                  {filters.sortBy === "sensorType" &&
                    (filters.sortOrder === "asc" ? "▲" : "▼")}
                </th>
                <th onClick={() => handleSort("timestamp")}>
                  Timestamp{" "}
                  {filters.sortBy === "timestamp" &&
                    (filters.sortOrder === "asc" ? "▲" : "▼")}
                </th>
                <th onClick={() => handleSort("value")}>
                  Value{" "}
                  {filters.sortBy === "value" &&
                    (filters.sortOrder === "asc" ? "▲" : "▼")}
                </th>
                <th>Unit</th>
              </tr>
            </thead>
            <tbody>
              {paginatedData.map((data) => (
                <tr key={data.sensorId + data.timestamp}>
                  <td>{data.sensorId}</td>
                  <td>{data.sensorName}</td>
                  <td>{data.sensorType}</td>
                  <td>{new Date(data.timestamp).toLocaleString()}</td>
                  <td>{data.value}</td>
                  <td>{getUnit(data.sensorType)}</td>
                </tr>
              ))}
            </tbody>
          </table>

          <div className="pagination">
            {getPaginationRange().map((page, index) =>
              page === "..." ? (
                <span key={index} className="pagination-ellipsis">
                  ...
                </span>
              ) : (
                <button
                  key={index}
                  className={currentPage === page ? "active" : ""}
                  onClick={() => handlePageChange(page)}
                >
                  {page}
                </button>
              ),
            )}
          </div>
        </>
      ) : (
        <div className="charts">
          {allChartData.map((chartData, index) => (
            <div
              key={index}
              className={`chart-container ${isSingleChart ? "fullscreen" : "small"}`}
              style={{ width: isSingleChart ? "100%" : "auto" }}
            >
              <h5>{`Chart for ${chartData.datasets[0].sensorType}`}</h5>
              <Line data={chartData} options={options} />
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default SensorDataTable;
