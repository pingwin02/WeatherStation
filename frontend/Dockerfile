FROM node:18 AS build
WORKDIR /app
COPY frontend/package.json /app/
RUN npm install
COPY frontend /app/
RUN npm run build

FROM nginx:stable-alpine AS prod
COPY --from=build /app/build /usr/share/nginx/html
COPY frontend/nginx.conf /etc/nginx/nginx.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]