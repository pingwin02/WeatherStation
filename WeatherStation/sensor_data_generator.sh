#!/bin/bash

QUEUE_NAME="sensor_data"
RABBITMQ_HOST="localhost"
RABBITMQ_USER="admin"
RABBITMQ_PASS="admin"
RABBITMQ_PORT="5672"

SENSOR_NAMES=("Temperature" "Humidity" "CO2" "Rainfall")

generate_sensor_data() {
    SENSOR_NAME=$1
    SENSOR_TYPE=$2
    SENSOR_VALUE=$((RANDOM % 100))

    SENSOR_DATA=$(cat <<EOF
{
    "SensorName": "$SENSOR_NAME",
    "SensorType": "$SENSOR_TYPE",
    "SensorValue": "$SENSOR_VALUE"
}
EOF
)
    echo $SENSOR_DATA
}

send_to_rabbitmq() {
    SENSOR_DATA=$1

    curl -u $RABBITMQ_USER:$RABBITMQ_PASS -X POST -d "$SENSOR_DATA" \
        "http://$RABBITMQ_HOST:$RABBITMQ_PORT/api/exchanges/%2F/$QUEUE_NAME/publish" \
        -H "content-type:application/json"
}

while true; do
    for ((i=1; i<=16; i++)); do
        SENSOR_NAME=${SENSOR_NAMES[$((i % 4))]}
        SENSOR_DATA=$(generate_sensor_data $SENSOR_NAME "sensor_$i")

        echo "Sending data for $SENSOR_NAME, Sensor $i: $SENSOR_DATA"
        send_to_rabbitmq "$SENSOR_DATA"
        
        sleep 1
    done
done
