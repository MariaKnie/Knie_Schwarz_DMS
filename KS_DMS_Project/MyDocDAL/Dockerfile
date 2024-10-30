# Staging: Base Image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8082

# Staging: Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Kopiert nur die .csproj-Datei aus dem entsprechenden Ordner
COPY ["./MyDocDAL/MyDocDAL.csproj", "./MyDocDAL/"]
RUN dotnet restore "./MyDocDAL/MyDocDAL.csproj"

# Kopiere alle anderen Dateien
COPY . .

# Build der Anwendung
RUN dotnet build "./MyDocDAL/MyDocDAL.csproj" -c Release -o /app/build

# Staging: Publish Stage
FROM build AS publish
RUN dotnet publish "./MyDocDAL/MyDocDAL.csproj" -c Release -o /app/publish

# Staging: Final Stage
FROM base AS final
WORKDIR /app

# Kopiere veröffentlichte Dateien aus dem vorherigen Staging
COPY --from=publish /app/publish .

# Copy and rename the log4net configuration file
COPY ./MyDocDAL/log4netDAL.config /app/log4net.config


# Start der Anwendung
ENTRYPOINT ["dotnet", "MyDocDAL.dll"]
