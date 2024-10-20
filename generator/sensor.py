import pika
import json
import time
import random
import requests
import threading
from datetime import datetime
from typing import List

RABBITMQ_HOST = "localhost"
RABBITMQ_QUEUE = "data_queue"
RABBITMQ_USER = "admin"
RABBITMQ_PASSWORD = "admin"
API_BASE_URL = "http://localhost:8000/api/sensors"
SENSORS_FOR_EACH_TYPE = 4


class Sensor:

    STOP_SIMULATION = threading.Event()

    SENSOR_INFO = {
        "Temperature": {"range": (-20, 50), "rate": 3},
        "Humidity": {"range": (0, 100), "rate": 2},
        "Pressure": {"range": (900, 1100), "rate": 3},
        "WindSpeed": {"range": (0, 150), "rate": 2},
    }

    ADDRESSES = [
        "0xb59ab3f970befdd1e1b009376083fb69c339d5ba",
        "0x16f870c925fe2399786d318b627e9c159e13fd38",
        "0x5e5ba4ae271de64e839421701ec4420d32ccaf0d",
        "0xfa48f36ba6cc563eef20eca4edbfc92d29b521cb",
        "0xad04b6f85210f7bac634972e32fa0e99c487b1f0",
        "0xc6b43723dc57e444ee3144e5db34453ba4b7fe58",
        "0xd65bb6b3b2b492feaf6d176721b5950b39b2ef5d",
        "0xced537ef904039c5f1218069b10992d5727b4042",
        "0xdee8acea1e0475cba90300dabdbf9393dd200ab6",
        "0xbadc41713a33e692c3a8f0688184d42c314ae20f",
        "0x6ffb0279e0c65990ee1af6ceba6903296fa960a7",
        "0x9108f1907295aec304fe9d23b4f022be9e509928",
        "0xbd213a69d31f225bf20442782634ed74f4655b7a",
        "0x8c2683e6a9de0b84ca9aee43c6a0f8fa40c99113",
        "0x16acba43ff56d02e2c583a6803d21045068677de",
        "0xdb54b838fe344527af40791bbdbea02cce1e0978",
    ]

    def __init__(
        self, sensor_id: str = None, sensor_type: str = None, sensor_name: str = None
    ):
        self.sensor_id = sensor_id
        self.sensor_type = sensor_type
        self.sensor_name = sensor_name

    @staticmethod
    def calculate_sleep_time(sensor_type: str) -> float:
        rate_per_minute = Sensor.SENSOR_INFO[sensor_type]["rate"]
        return 60 / rate_per_minute

    def generate_sensor_data(self, value=None) -> str:
        sensor_value = (
            value
            if value is not None
            else random.uniform(*Sensor.SENSOR_INFO[self.sensor_type]["range"])
        )
        sensor_data = {
            "sensorId": str(self.sensor_id),
            "value": sensor_value,
            "timestamp": datetime.utcnow().isoformat(),
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
                credentials = pika.PlainCredentials(
                    username=RABBITMQ_USER, password=RABBITMQ_PASSWORD
                )
                connection_params = pika.ConnectionParameters(
                    host=RABBITMQ_HOST, credentials=credentials
                )
                connection = pika.BlockingConnection(connection_params)
                channel = connection.channel()
                channel.queue_declare(queue=RABBITMQ_QUEUE)
                channel.basic_publish(
                    exchange="", routing_key=RABBITMQ_QUEUE, body=sensor_data
                )
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


def create(sensor_name: str, sensor_type: str, wallet_address: str) -> None:
    sensor_data = {
        "name": sensor_name,
        "type": sensor_type,
        "wallet_address": wallet_address,
    }
    response = requests.post(API_BASE_URL, json=sensor_data)
    if response.status_code != 201:
        raise Exception(f"Failed to create sensor {response}")


def get_all_sensors() -> List[dict]:
    response = requests.get(API_BASE_URL)
    if response.status_code == 200:
        return response.json()
    else:
        raise Exception(f"Failed to get all sensors {response}")


def create_sensors() -> None:
    print("Creating sensors...")
    iterator = 0
    for sensor_type in Sensor.SENSOR_INFO.keys():
        for i in range(SENSORS_FOR_EACH_TYPE):
            sensor_name = f"{sensor_type[:4].lower()}#{i+1}"
            create(sensor_name, sensor_type, Sensor.ADDRESSES[iterator])
            iterator += 1


def delete_all_sensors() -> None:
    print("Deleting remaining sensors...")
    all_sensors = get_all_sensors()
    for sensor in all_sensors:
        sensor_id = sensor["id"]
        response = requests.delete(f"{API_BASE_URL}/{sensor_id}")
        if response.status_code != 204:
            raise Exception(f"Failed to delete sensor {sensor_id} {response}")
