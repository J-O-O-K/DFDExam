# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
# For more information, please see https://aka.ms/containercompat

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy shared contracts
COPY ["src/Shared/TaskManagement.Contracts/TaskManagement.Contracts.csproj", "src/Shared/TaskManagement.Contracts/"]

# Copy AnalyticsService projects
COPY ["src/Services/AnalyticsService/AnalyticsService.API/AnalyticsService.API.csproj", "src/Services/AnalyticsService/AnalyticsService.API/"]
COPY ["src/Services/AnalyticsService/AnalyticsService.Application/AnalyticsService.Application.csproj", "src/Services/AnalyticsService/AnalyticsService.Application/"]
COPY ["src/Services/AnalyticsService/AnalyticsService.Domain/AnalyticsService.Domain.csproj", "src/Services/AnalyticsService/AnalyticsService.Domain/"]
COPY ["src/Services/AnalyticsService/AnalyticsService.Infrastructure/AnalyticsService.Infrastructure.csproj", "src/Services/AnalyticsService/AnalyticsService.Infrastructure/"]
COPY ["src/Services/AnalyticsService/AnalyticsService.Persistence/AnalyticsService.Persistence.csproj", "src/Services/AnalyticsService/AnalyticsService.Persistence/"]

# Restore dependencies
RUN dotnet restore "src/Services/AnalyticsService/AnalyticsService.API/AnalyticsService.API.csproj"

# Copy all source code
COPY . .

WORKDIR "/src/src/Services/AnalyticsService/AnalyticsService.API"
RUN dotnet build "AnalyticsService.API.csproj" -c Release -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
RUN dotnet publish "AnalyticsService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AnalyticsService.API.dll"]