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

# Copy TaskService projects
COPY ["src/Services/TaskService/TaskService.API/TaskService.API.csproj", "src/Services/TaskService/TaskService.API/"]
COPY ["src/Services/TaskService/TaskService.Application/TaskService.Application.csproj", "src/Services/TaskService/TaskService.Application/"]
COPY ["src/Services/TaskService/TaskService.Domain/TaskService.Domain.csproj", "src/Services/TaskService/TaskService.Domain/"]
COPY ["src/Services/TaskService/TaskService.Infrastructure/TaskService.Infrastructure.csproj", "src/Services/TaskService/TaskService.Infrastructure/"]
COPY ["src/Services/TaskService/TaskService.Persistence/TaskService.Persistence.csproj", "src/Services/TaskService/TaskService.Persistence/"]

# Restore dependencies
RUN dotnet restore "src/Services/TaskService/TaskService.API/TaskService.API.csproj"

# Copy all source code
COPY . .

WORKDIR "/src/src/Services/TaskService/TaskService.API"
RUN dotnet build "TaskService.API.csproj" -c Release -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
RUN dotnet publish "TaskService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskService.API.dll"]