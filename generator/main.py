import time
from sensor import Sensor, get_all_sensors, create_sensors, delete_all_sensors
from concurrent.futures import ThreadPoolExecutor, Future
import argparse

ALL_SENSORS = 16


def start_simulation():
    all_sensors = get_all_sensors()
    sensors_info = [
        Sensor(sensor_data["id"], sensor_data["type"], sensor_data["name"])
        for sensor_data in all_sensors
    ]

    print("Starting simulation...")
    with ThreadPoolExecutor(max_workers=ALL_SENSORS) as executor:
        futures: dict[Future, Sensor] = {
            executor.submit(sensor.send_data): sensor for sensor in sensors_info
        }

        try:
            while not Sensor.STOP_SIMULATION.is_set():
                time.sleep(1)
        except KeyboardInterrupt:
            print("Stopping simulation. Please wait...")
            Sensor.STOP_SIMULATION.set()

    print("Simulation stopped. All threads finished.")


def send_single_value(sensor_id: str, value: float):
    all_sensors = get_all_sensors()
    sensor_data = next(
        (sensor for sensor in all_sensors if sensor["id"] == sensor_id), None
    )
    if sensor_data:
        sensor = Sensor(sensor_data["id"], sensor_data["type"], sensor_data["name"])
        sensor.send_data(value)
    else:
        print(f"Sensor {sensor_id} not found.")


def main():
    parser = argparse.ArgumentParser(
        description="Sensor data simulation and management."
    )
    parser.add_argument(
        "--single",
        metavar="id:value",
        type=str,
        help="send a single value by a sensor (format: sensor_id:value)",
        default=None,
    )
    parser.add_argument(
        "--create", action="store_true", help="create sensors", default=False
    )
    parser.add_argument(
        "--start", action="store_true", help="start the simulation", default=False
    )
    parser.add_argument(
        "--delete", action="store_true", help="delete all sensors", default=False
    )

    args = parser.parse_args()

    if args.single:
        sensor_name, value = args.single.split(":")
        send_single_value(sensor_name, float(value))
    elif args.create:
        create_sensors()
    elif args.start:
        start_simulation()
    elif args.delete:
        delete_all_sensors()
    else:
        delete_all_sensors()
        create_sensors()
        start_simulation()


if __name__ == "__main__":
    main()
