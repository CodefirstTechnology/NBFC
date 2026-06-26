# See https://aka.ms/customizecontainer

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Build.props", "./"]
COPY ["src/Host/Patsanstha.Api/Patsanstha.Api.csproj", "src/Host/Patsanstha.Api/"]
COPY ["src/Shared/BuildingBlocks/Patsanstha.BuildingBlocks.Infrastructure/Patsanstha.BuildingBlocks.Infrastructure.csproj", "src/Shared/BuildingBlocks/Patsanstha.BuildingBlocks.Infrastructure/"]
COPY ["src/Shared/BuildingBlocks/Patsanstha.BuildingBlocks.Application/Patsanstha.BuildingBlocks.Application.csproj", "src/Shared/BuildingBlocks/Patsanstha.BuildingBlocks.Application/"]
COPY ["src/Shared/BuildingBlocks/Patsanstha.BuildingBlocks.Domain/Patsanstha.BuildingBlocks.Domain.csproj", "src/Shared/BuildingBlocks/Patsanstha.BuildingBlocks.Domain/"]
COPY ["src/Shared/Contracts/Patsanstha.Contracts/Patsanstha.Contracts.csproj", "src/Shared/Contracts/Patsanstha.Contracts/"]
RUN dotnet restore "src/Host/Patsanstha.Api/Patsanstha.Api.csproj"
COPY . .
WORKDIR "/src/src/Host/Patsanstha.Api"
RUN dotnet build "Patsanstha.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Patsanstha.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Patsanstha.Api.dll"]
