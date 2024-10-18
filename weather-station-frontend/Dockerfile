FROM node:14 AS build
WORKDIR /app
COPY weather-station-frontend/package.json /app/
RUN npm install
COPY weather-station-frontend /app/

CMD ["npm", "start"]