FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Gateways/PantryCloud.ApiGateway/PantryCloud.ApiGateway.Presentation/PantryCloud.ApiGateway.Presentation.csproj", "src/Gateways/PantryCloud.ApiGateway/PantryCloud.ApiGateway.Presentation/"]
COPY ["src/Gateways/PantryCloud.ApiGateway/PantryCloud.ApiGateway.Infrastructure/PantryCloud.ApiGateway.Infrastructure.csproj", "src/Gateways/PantryCloud.ApiGateway/PantryCloud.ApiGateway.Infrastructure/"]
COPY ["src/Gateways/PantryCloud.ApiGateway/PantryCloud.ApiGateway.Core/PantryCloud.ApiGateway.Core.csproj", "src/Gateways/PantryCloud.ApiGateway/PantryCloud.ApiGateway.Core/"]
COPY ["src/BuildingBlocks/PantryCloud.SharedKernel/PantryCloud.SharedKernel.csproj", "src/BuildingBlocks/PantryCloud.SharedKernel/"]
RUN dotnet restore "src/Gateways/PantryCloud.ApiGateway/PantryCloud.ApiGateway.Presentation/PantryCloud.ApiGateway.Presentation.csproj"
COPY . .
WORKDIR "/src/src/Gateways/PantryCloud.ApiGateway/PantryCloud.ApiGateway.Presentation"
RUN dotnet build "./PantryCloud.ApiGateway.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PantryCloud.ApiGateway.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PantryCloud.ApiGateway.Presentation.dll"]

