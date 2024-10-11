// init-mongo.js

db = db.getSiblingDB('WeatherStation');
db.dropDatabase();

db.sensors.insertMany([
    { Name: "Temperature", SensorNumber: 1, SensorValue: 22.5 },
    { Name: "Temperature", SensorNumber: 2, SensorValue: 23.1 },
    { Name: "Temperature", SensorNumber: 3, SensorValue: 22.7 },
    { Name: "Temperature", SensorNumber: 4, SensorValue: 23.8 },
    { Name: "Humidity", SensorNumber: 5, SensorValue: 63.0 },
    { Name: "Humidity", SensorNumber: 6, SensorValue: 62.2 },
    { Name: "Humidity", SensorNumber: 7, SensorValue: 63.1 },
    { Name: "Humidity", SensorNumber: 8, SensorValue: 62.8 },
    { Name: "CO2",  SensorNumber: 9, SensorValue: 400.0 },
    { Name: "CO2",  SensorNumber: 10, SensorValue: 391.0 },
    { Name: "CO2",  SensorNumber: 11, SensorValue: 401.0 },
    { Name: "CO2",  SensorNumber: 12, SensorValue: 305.0 },
    { Name: "UVIntensity", SensorNumber: 13, SensorValue: 5.1 },
    { Name: "UVIntensity", SensorNumber: 14, SensorValue: 4.9 },
    { Name: "UVIntensity", SensorNumber: 15, SensorValue: 4.9 },
    { Name: "UVIntensity", SensorNumber: 16, SensorValue: 5.0 }
]);