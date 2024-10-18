import React from 'react';

const FilterForm = ({ filter, setFilter }) => {

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFilter(prevState => ({
            ...prevState,
            [name]: value
        }));
    };

    return (
        <form>
            <label>Sensor Type</label>
            <input type="text" name="sensorType" value={filter.sensorType} onChange={handleChange} />

            <label>Sensor Instance</label>
            <input type="text" name="sensorInstance" value={filter.sensorInstance} onChange={handleChange} />

            <label>Start Date</label>
            <input type="date" name="startDate" value={filter.startDate} onChange={handleChange} />

            <label>End Date</label>
            <input type="date" name="endDate" value={filter.endDate} onChange={handleChange} />
        </form>
    );
};

export default FilterForm;
