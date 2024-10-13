import pika
import json
import time
import random
import requests
from datetime import datetime
from typing import List
from concurrent.futures import ThreadPoolExecutor, Future
import threading

RABBITMQ_HOST = 'localhost'
RABBITMQ_QUEUE = 'data_queue'
RABBITMQ_USER = 'admin'
RABBITMQ_PASSWORD = 'admin'

API_BASE_URL = 'http://localhost:8000/api/sensors'

TYPE1 = "Temperature"
TYPE2 = "Humidity"
TYPE3 = "Pressure"
TYPE4 = "WindSpeed"

SENSOR_TYPES = [TYPE1, TYPE2, TYPE3, TYPE4]
SENSORS_FOR_EACH_TYPE = 4

SENSOR_RANGES = {
    TYPE1: (-20, 50),
    TYPE2: (0, 100),
    TYPE3: (900, 1100),
    TYPE4: (0, 150)
}

DATA_GENERATION_RATE = {
    TYPE1: 5,
    TYPE2: 2,
    TYPE3: 3,
    TYPE4: 4
}

stop_simulation = threading.Event()

class Sensor:
    def __init__(self, sensor_id: str = None, sensor_type: str = None, sensor_name: str = None):
        self.sensor_id = sensor_id
        self.sensor_type = sensor_type
        self.sensor_name = sensor_name

    @staticmethod
    def calculate_sleep_time(sensor_type: str) -> float:
        rate_per_minute = DATA_GENERATION_RATE[sensor_type]
        return 60 / rate_per_minute

    def generate_sensor_data(self) -> str:
        sensor_value = random.uniform(SENSOR_RANGES[self.sensor_type][0], SENSOR_RANGES[self.sensor_type][1])
        sensor_data = {
            "sensorId": str(self.sensor_id),
            "value": sensor_value,
            "timestamp": datetime.now().isoformat()
        }
        return json.dumps(sensor_data)

    def send_data(self) -> None:
        sleep_time = self.calculate_sleep_time(self.sensor_type)
        initial_delay = random.uniform(0.5, 2)
        time.sleep(initial_delay)
        
        while not stop_simulation.is_set():
            sensor_data = self.generate_sensor_data()
            try:
                credentials = pika.PlainCredentials(username=RABBITMQ_USER, password=RABBITMQ_PASSWORD)
                connection_params = pika.ConnectionParameters(host=RABBITMQ_HOST, credentials=credentials)
                connection = pika.BlockingConnection(connection_params)
                channel = connection.channel()
                channel.queue_declare(queue=RABBITMQ_QUEUE)
                channel.basic_publish(exchange='', routing_key=RABBITMQ_QUEUE, body=sensor_data)
                print(f"Sent from sensor {self.sensor_name}: {sensor_data}")
                time.sleep(sleep_time)
            except Exception as e:
                if stop_simulation.is_set():
                    break
                print(f"Error sending data for sensor {self.sensor_id}: {e}")
            finally:
                if connection.is_open:
                    connection.close()

    @staticmethod
    def create(sensor_name: str, sensor_type: str) -> None:
        sensor_data = {
            "name": sensor_name,
            "type": sensor_type
        }
        response = requests.post(API_BASE_URL, json=sensor_data)
        if response.status_code != 201:
            raise Exception(f"Failed to create sensor {response}")

    @staticmethod
    def get_all_sensors() -> List[dict]:
        response = requests.get(API_BASE_URL)
        if response.status_code == 200:
            return response.json()
        else:
            raise Exception(f"Failed to get all sensors {response}")

    @staticmethod
    def delete_all_sensors() -> None:
        print("Deleting remaining sensors...")
        all_sensors = Sensor.get_all_sensors()
        for sensor in all_sensors:
            sensor_id = sensor["id"]
            response = requests.delete(f"{API_BASE_URL}/{sensor_id}")
            if response.status_code != 204:
                raise Exception(f"Failed to delete sensor {sensor_id} {response}")

def main() -> None:
    Sensor.delete_all_sensors()

    ready = input("Are you ready to start the simulation? (y/n): ")
    if ready.lower() != 'y':
        print("Exiting...")
        return

    print("Creating sensors...")
    sensors_info: List[Sensor] = []
    for sensor_type in SENSOR_TYPES:
        for i in range(SENSORS_FOR_EACH_TYPE):
            sensor_name = f"{sensor_type[:4].lower()}#{i+1}"
            Sensor.create(sensor_name, sensor_type)

    all_sensors = Sensor.get_all_sensors()
    for sensor_data in all_sensors:
        sensor = Sensor(sensor_data["id"], sensor_data["type"], sensor_data["name"])
        sensors_info.append(sensor)

    print("Starting simulation...")
    with ThreadPoolExecutor(max_workers=16) as executor:
        futures: dict[Future, Sensor] = {executor.submit(sensor.send_data): sensor for sensor in sensors_info}

        try:
            while not stop_simulation.is_set():
                time.sleep(1)
        except KeyboardInterrupt:
            print("Stopping simulation. Please wait...")
            stop_simulation.set()

    print("Simulation stopped. All threads finished.")

if __name__ == "__main__":
    main()
