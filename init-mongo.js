// init-mongo.js

db = db.getSiblingDB('WeatherStation');
db.dropDatabase();

db.sensors.insertMany([
    { name: "Temperature", number: 1, value: 22.5, created_at: new Date() },
    { name: "Temperature", number: 2, value: 23.1, created_at: new Date() },
    { name: "Temperature", number: 3, value: 22.7, created_at: new Date() },
    { name: "Temperature", number: 4, value: 23.8, created_at: new Date() },
    { name: "Humidity", number: 5, value: 63.0, created_at: new Date() },
    { name: "Humidity", number: 6, value: 62.2, created_at: new Date() },
    { name: "Humidity", number: 7, value: 63.1, created_at: new Date() },
    { name: "Humidity", number: 8, value: 62.8, created_at: new Date() },
    { name: "CO2",  number: 9, value: 400.0, created_at: new Date() },
    { name: "CO2",  number: 10, value: 391.0, created_at: new Date() },
    { name: "CO2",  number: 11, value: 401.0, created_at: new Date() },
    { name: "CO2",  number: 12, value: 305.0, created_at: new Date() },
    { name: "UVIntensity", number: 13, value: 5.1, created_at: new Date() },
    { name: "UVIntensity", number: 14, value: 4.9, created_at: new Date() },
    { name: "UVIntensity", number: 15, value: 4.9, created_at: new Date() },
    { name: "UVIntensity", number: 16, value: 5.0, created_at: new Date() }
]);