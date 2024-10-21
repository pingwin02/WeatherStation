FROM node:18 AS build
WORKDIR /app
COPY frontend/package.json /app/
RUN npm install --omit=dev
ENV NODE_ENV=production
COPY frontend /app/

CMD ["npm", "start"]