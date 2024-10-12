FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 8000
EXPOSE 443

ADD https://raw.githubusercontent.com/vishnubob/wait-for-it/master/wait-for-it.sh /app/wait-for-it.sh
RUN chmod +x /app/wait-for-it.sh

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["WeatherStationBackend/WeatherStationBackend.csproj", "WeatherStationBackend/"]
RUN dotnet restore "WeatherStationBackend/WeatherStationBackend.csproj"
COPY . .
WORKDIR "/src/WeatherStationBackend"
RUN dotnet build "WeatherStationBackend.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "WeatherStationBackend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["/bin/sh", "-c", "/app/wait-for-it.sh rabbitmq:5672 -- dotnet WeatherStationBackend.dll"]
