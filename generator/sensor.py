import pika
import json
import time
import random
import requests
import threading
from datetime import datetime
from typing import List

RABBITMQ_HOST = 'localhost'
RABBITMQ_QUEUE = 'data_queue'
RABBITMQ_USER = 'admin'
RABBITMQ_PASSWORD = 'admin'
API_BASE_URL = 'http://localhost:8000/api/sensors'

class Sensor:

    STOP_SIMULATION = threading.Event()

    SENSOR_INFO = {
        "Temperature": {
            "range": (-20, 50),
            "rate": 3
        },
        "Humidity": {
            "range": (0, 100),
            "rate": 1
        },
        "Pressure": {
            "range": (900, 1100),
            "rate": 3
        },
        "WindSpeed": {
            "range": (0, 150),
            "rate": 2
        }
    }

    def __init__(self, sensor_id: str = None, sensor_type: str = None, sensor_name: str = None):
        self.sensor_id = sensor_id
        self.sensor_type = sensor_type
        self.sensor_name = sensor_name

    @staticmethod
    def calculate_sleep_time(sensor_type: str) -> float:
        rate_per_minute = Sensor.SENSOR_INFO[sensor_type]["rate"]
        return 60 / rate_per_minute

    def generate_sensor_data(self, value=None) -> str:
        sensor_value = value if value is not None else random.uniform(*Sensor.SENSOR_INFO[self.sensor_type]["range"])
        sensor_data = {
            "sensorId": str(self.sensor_id),
            "value": sensor_value,
            "timestamp": datetime.utcnow().isoformat()
        }
        return json.dumps(sensor_data)

    def send_data(self, value=None) -> None:
        if value is None:
            sleep_time = self.calculate_sleep_time(self.sensor_type)
            initial_delay = random.uniform(0, sleep_time)
            time.sleep(initial_delay)

        while not self.STOP_SIMULATION.is_set():
            sensor_data = self.generate_sensor_data(value)
            try:
                credentials = pika.PlainCredentials(username=RABBITMQ_USER, password=RABBITMQ_PASSWORD)
                connection_params = pika.ConnectionParameters(host=RABBITMQ_HOST, credentials=credentials)
                connection = pika.BlockingConnection(connection_params)
                channel = connection.channel()
                channel.queue_declare(queue=RABBITMQ_QUEUE)
                channel.basic_publish(exchange='', routing_key=RABBITMQ_QUEUE, body=sensor_data)
                print(f"Sent from sensor {self.sensor_name}: {sensor_data}")
                if value is None:
                    time.sleep(sleep_time)
                else:
                    break
            except Exception as e:
                print(f"Error sending data for sensor {self.sensor_id}: {e}")
            finally:
                if connection.is_open:
                    connection.close()

        print(f"Stopping simulation for sensor {self.sensor_name}...")

    @staticmethod
    def create(sensor_name: str, sensor_type: str, wallet_address: str) -> None:
        sensor_data = {
            "name": sensor_name,
            "type": sensor_type,
            "wallet_address": wallet_address,
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
