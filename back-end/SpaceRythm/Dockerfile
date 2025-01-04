## Базовий образ для ASP.NET
#FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
#WORKDIR /app
#EXPOSE 8080
#EXPOSE 8081

# Stage 1: Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release

# Set the working directory
WORKDIR /src

# Copy the project file
COPY ["SpaceRythm.csproj", "./"]

# Restore dependencies
RUN dotnet restore "SpaceRythm.csproj"

# Copy the rest of the files
COPY . .

# Build the project
RUN dotnet build "SpaceRythm.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage 2: Publish stage
FROM build AS publish
RUN dotnet publish "SpaceRythm.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 3: Final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SpaceRythm.dll"]