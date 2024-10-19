import time
from sensor import Sensor
from concurrent.futures import ThreadPoolExecutor, Future
import argparse

SENSORS_FOR_EACH_TYPE = 4
ALL_SENSORS = 16
ADDRESSES = [
    "0xB59aB3f970befDd1e1B009376083FB69c339D5ba",
    "0x16f870C925Fe2399786d318B627E9c159e13FD38",
    "0x5E5Ba4Ae271De64E839421701Ec4420D32CCAf0D",
    "0xfa48f36BA6Cc563eEf20eCA4eDBfc92d29b521cB",
    "0xaD04B6f85210F7BAc634972e32fa0E99C487B1f0",
    "0xc6B43723dC57E444Ee3144E5Db34453bA4b7fe58",
    "0xD65bb6b3B2B492feAf6d176721b5950B39b2EF5d",
    "0xCed537Ef904039c5f1218069b10992D5727B4042",
    "0xDEE8acEA1E0475cbA90300DabDbf9393dD200Ab6",
    "0xBADC41713A33E692c3A8f0688184d42c314AE20f",
    "0x6ffB0279E0C65990eE1aF6CeBA6903296fa960a7",
    "0x9108f1907295Aec304FE9d23B4f022BE9e509928",
    "0xBD213a69d31F225Bf20442782634ed74F4655B7a",
    "0x8c2683e6a9dE0B84cA9AEE43C6a0F8fa40c99113",
    "0x16ACba43Ff56D02E2c583A6803d21045068677DE",
    "0xDB54b838Fe344527AF40791bbDbea02cce1e0978"
]

def create_sensors():
    print("Creating sensors...")
    iterator = 0
    for sensor_type in Sensor.SENSOR_INFO.keys():
        for i in range(SENSORS_FOR_EACH_TYPE):
            sensor_name = f"{sensor_type[:4].lower()}#{i+1}"
            Sensor.create(sensor_name, sensor_type, ADDRESSES[iterator])
            iterator += 1

def start_simulation():
    all_sensors = Sensor.get_all_sensors()
    sensors_info = [Sensor(sensor_data["id"], sensor_data["type"], sensor_data["name"]) for sensor_data in all_sensors]

    print("Starting simulation...")
    with ThreadPoolExecutor(max_workers=ALL_SENSORS) as executor:
        futures: dict[Future, Sensor] = {executor.submit(sensor.send_data): sensor for sensor in sensors_info}

        try:
            while not Sensor.STOP_SIMULATION.is_set():
                time.sleep(1)
        except KeyboardInterrupt:
            print("Stopping simulation. Please wait...")
            Sensor.STOP_SIMULATION.set()

    print("Simulation stopped. All threads finished.")

def send_single_value(sensor_id: str, value: float):
    all_sensors = Sensor.get_all_sensors()
    sensor_data = next((sensor for sensor in all_sensors if sensor["id"] == sensor_id), None)
    if sensor_data:
        sensor = Sensor(sensor_data["id"], sensor_data["type"], sensor_data["name"])
        sensor.send_data(value)
    else:
        print(f"Sensor {sensor_id} not found.")

def main():
    parser = argparse.ArgumentParser(description="Sensor data simulation and management.")
    parser.add_argument('--single', type=str, help="Send a single value to a sensor (format: sensor_id:value)", default=None)
    parser.add_argument('--create', action='store_true', help="Only create sensors and exit", default=False)
    parser.add_argument('--start', action='store_true', help="Only start the simulation", default=False)
    
    args = parser.parse_args()

    if args.single:
        sensor_name, value = args.single.split(":")
        send_single_value(sensor_name, float(value))
    elif args.create:
        create_sensors()
    elif args.start:
        start_simulation()
    else:
        Sensor.delete_all_sensors()
        if input("Are you ready to start the simulation? (y/n): ").lower() != 'y':
            print("Exiting...")
            return
        create_sensors()
        start_simulation()

if __name__ == "__main__":
    main()
