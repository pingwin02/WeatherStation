import pika
import json
import time
import random

RABBITMQ_HOST = 'localhost'  
RABBITMQ_QUEUE = 'sensor_queue'
RABBITMQ_USER = 'admin' 
RABBITMQ_PASSWORD = 'admin' 

SENSOR_NAMES = ["Temperature", "Humidity", "CO2", "UVIntensity"]
SENSOR_RANGES = {"Temperature": (-20, 20), "Humidity": (0, 100), "CO2": (0, 2000), "UVIntensity": (0, 15)}

def generate_sensor_data(sensor_id):
    sensor_name = SENSOR_NAMES[sensor_id % len(SENSOR_NAMES)]
    sensor_value = random.randint(SENSOR_RANGES[sensor_name][0], SENSOR_RANGES[sensor_name][1])
    sensor_data = {
        "Name": sensor_name,
        "SensorNumber": sensor_id,
        "SensorValue": sensor_value
    }
    return json.dumps(sensor_data)

credentials = pika.PlainCredentials(username=RABBITMQ_USER, password=RABBITMQ_PASSWORD)
connection_params = pika.ConnectionParameters(
    host=RABBITMQ_HOST,
    credentials=credentials
)

connection = pika.BlockingConnection(connection_params)
channel = connection.channel()
channel.queue_declare(queue=RABBITMQ_QUEUE)

try:
    while True:
        for sensor_id in range(1, 17):
            sensor_data = generate_sensor_data(sensor_id)
            channel.basic_publish(exchange='',
                                  routing_key=RABBITMQ_QUEUE,
                                  body=sensor_data)
            print(f"Sent: {sensor_data}")
            time.sleep(1)
except KeyboardInterrupt:
    print("Stopping simulation...")
finally:
    connection.close()
