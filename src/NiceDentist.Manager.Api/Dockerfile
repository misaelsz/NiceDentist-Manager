# Use the official .NET runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/NiceDentist.Manager.Api/NiceDentist.Manager.Api.csproj", "src/NiceDentist.Manager.Api/"]
COPY ["src/NiceDentist.Manager.Application/NiceDentist.Manager.Application.csproj", "src/NiceDentist.Manager.Application/"]
COPY ["src/NiceDentist.Manager.Domain/NiceDentist.Manager.Domain.csproj", "src/NiceDentist.Manager.Domain/"]
COPY ["src/NiceDentist.Manager.Infrastructure/NiceDentist.Manager.Infrastructure.csproj", "src/NiceDentist.Manager.Infrastructure/"]

RUN dotnet restore "src/NiceDentist.Manager.Api/NiceDentist.Manager.Api.csproj"

# Copy the rest of the source code
COPY . .

# Build the application
WORKDIR "/src/src/NiceDentist.Manager.Api"
RUN dotnet build "NiceDentist.Manager.Api.csproj" -c "$BUILD_CONFIGURATION" -o /app/build

# Publish the application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "NiceDentist.Manager.Api.csproj" -c "$BUILD_CONFIGURATION" -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create a non-root user for security
RUN adduser --disabled-password --gecos "" --uid 1000 appuser && chown -R appuser:appuser /app
USER appuser

ENTRYPOINT ["dotnet", "NiceDentist.Manager.Api.dll"]
