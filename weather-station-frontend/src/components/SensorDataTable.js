import React, { useEffect, useState } from 'react';
import { getSensors, getData } from '../services/sensorService';
import './styles/SensorDataTable.css'; // Import for CSS styling

const SensorDataTable = () => {
    const [allData, setAllData] = useState([]); // Pełny zbiór danych
    const [filteredData, setFilteredData] = useState([]); // Filtrowany zbiór danych
    const [currentPage, setCurrentPage] = useState(1); // Aktualna strona
    const [filters, setFilters] = useState({
        sensorName: '',
        sensorType: '',
        startDate: '',
        endDate: ''
    });
    const [sortConfig, setSortConfig] = useState({ key: '', direction: 'asc' }); // Stan sortowania

    const itemsPerPage = 16; // Liczba pomiarów na stronę

    // Funkcja pobierająca dane czujników i pomiary
    const fetchAllData = async () => {
        try {
            const sensors = await getSensors();
            if (!sensors) throw new Error('Sensor data is undefined');

            const data = await getData();
            if (!data) throw new Error('Data is undefined');

            // Połączenie sensorów z ich danymi pomiarowymi
            const mergedData = data.map(item => {
                const sensor = sensors.find(sensor => sensor.id === item.sensorId);
                return {
                    ...item,
                    sensorName: sensor ? sensor.name : 'Unknown',
                    sensorType: sensor ? sensor.type : 'Unknown'
                };
            });

            setAllData(mergedData);
            setFilteredData(mergedData); // Domyślnie brak filtrów
        } catch (error) {
            console.error('Error fetching data:', error);
        }
    };

    // Ładowanie danych przy montażu komponentu
    useEffect(() => {
        fetchAllData();
    }, []);

    // Funkcja filtrowania danych
    const filterData = () => {
        const { sensorName, sensorType, startDate, endDate } = filters;
        let filtered = allData;

        if (sensorName) {
            filtered = filtered.filter(data => data.sensorName.toLowerCase().includes(sensorName.toLowerCase()));
        }
        if (sensorType) {
            filtered = filtered.filter(data => data.sensorType.toLowerCase().includes(sensorType.toLowerCase()));
        }
        if (startDate) {
            filtered = filtered.filter(data => new Date(data.timestamp) >= new Date(startDate));
        }
        if (endDate) {
            filtered = filtered.filter(data => new Date(data.timestamp) <= new Date(endDate));
        }

        setFilteredData(filtered);
        setCurrentPage(1); // Resetowanie do pierwszej strony po filtrowaniu
    };

    // Obsługa zmiany w polach filtrów
    const handleFilterChange = (e) => {
        setFilters({
            ...filters,
            [e.target.name]: e.target.value
        });
    };

    // Uruchamianie filtrowania za każdym razem, gdy filtry się zmieniają
    useEffect(() => {
        filterData();
    }, [filters]);

    // Funkcja sortowania danych
    const sortData = (key) => {
        let direction = 'asc';
        if (sortConfig.key === key && sortConfig.direction === 'asc') {
            direction = 'desc';
        }
        setSortConfig({ key, direction });

        const sortedData = [...filteredData].sort((a, b) => {
            if (key === 'value') {
                return direction === 'asc' ? a.value - b.value : b.value - a.value;
            } else if (key === 'timestamp') {
                return direction === 'asc'
                    ? new Date(a.timestamp) - new Date(b.timestamp)
                    : new Date(b.timestamp) - new Date(a.timestamp);
            } else {
                const aValue = a[key] ? a[key].toString().toLowerCase() : '';
                const bValue = b[key] ? b[key].toString().toLowerCase() : '';
                if (aValue < bValue) return direction === 'asc' ? -1 : 1;
                if (aValue > bValue) return direction === 'asc' ? 1 : -1;
                return 0;
            }
        });

        setFilteredData(sortedData);
    };

    // Funkcja zmiany strony
    const handlePageChange = (newPage) => {
        setCurrentPage(newPage);
    };

    // Obliczanie indeksów do paginacji
    const startIndex = (currentPage - 1) * itemsPerPage;
    const paginatedData = filteredData.slice(startIndex, startIndex + itemsPerPage);
    const totalPages = Math.ceil(filteredData.length / itemsPerPage);

        // Funkcja zwracająca odpowiednią jednostkę dla każdego typu sensora
    const getUnit = (sensorType) => {
        switch (sensorType.toLowerCase()) {  // Added trim() to remove extra spaces
            case 'windspeed':
                return 'km/h';
            case 'pressure':
                return 'hPa';
            case 'temperature':
                return '°C';
            case 'humidity':
                return 'g/m³';
            default:
                return '';
        }
    };


    return (
        <div className="sensor-data-table">
            <h1>All Sensor Data</h1>

            {/* Formularz filtrowania */}
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

            {/* Tabela z danymi czujników */}
            <table className="sensor-table">
                <thead>
                    <tr>
                        <th onClick={() => sortData('sensorName')}>
                            Sensor Name {sortConfig.key === 'sensorName' ? (sortConfig.direction === 'asc' ? '▲' : '▼') : ''}
                        </th>
                        <th onClick={() => sortData('sensorType')}>
                            Sensor Type {sortConfig.key === 'sensorType' ? (sortConfig.direction === 'asc' ? '▲' : '▼') : ''}
                        </th>
                        <th onClick={() => sortData('value')}>
                            Value {sortConfig.key === 'value' ? (sortConfig.direction === 'asc' ? '▲' : '▼') : ''}
                        </th>
                        <th onClick={() => sortData('timestamp')}>
                            Timestamp {sortConfig.key === 'timestamp' ? (sortConfig.direction === 'asc' ? '▲' : '▼') : ''}
                        </th>
                    </tr>
                </thead>
                <tbody>
                    {paginatedData.length > 0 ? (
                        paginatedData.map((data) => (
                            <tr key={data.id}>
                                <td>{data.sensorName}</td>
                                <td>{data.sensorType}</td>
                                <td>
                                    <span title={`Unit: ${getUnit(data.sensorType)}`}>
                                        {data.value.toFixed(2)}
                                    </span>
                                </td>
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

            {/* Paginacja */}
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
