# WeatherStation

## To start the app:
1. Clone the repository
2. Run `docker compose up --build` in the root directory
3. Open `http://localhost:8000/api/sensors` in your browser
4. Go to the GeneratorScript directory
5. Run `python3 -m venv venv`
6. Run `source venv/bin/activate`
7. Run `pip install -r requirements.txt`
8. Run `python3 sensors_data_generator.py`
9. Refresh `http://localhost:8000/api/sensors` in your browser to see the results
10. To stop the app, run `docker compose down -v` in the root directory. It will remove the database volume as well.

## NOTE
When first starting the app, the database will be populated with example 16 sensor values. 

## Endpoints:

### Sensors:
- GET `/api/sensors` - get all sensors
- GET `/api/sensors/{id}` - get sensor by id
- POST `/api/sensors` - create a new sensor
- PUT `/api/sensors/{id}` - update sensor by id
- DELETE `/api/sensors/{id}` - delete sensor by id
- GET `/api/sensors/recent/{number}` - get most recent sensor value by its number (1 - 16)