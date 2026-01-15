FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/BuildingBlocks/PantryCloud.SharedKernel/PantryCloud.SharedKernel.csproj", "src/BuildingBlocks/PantryCloud.SharedKernel/"]
COPY ["src/Services/PantryCloud.Notification/PantryCloud.Notification.Presentation/PantryCloud.Notification.Presentation.csproj", "src/Services/PantryCloud.Notification/PantryCloud.Notification.Presentation/"]
COPY ["src/Services/PantryCloud.Notification/PantryCloud.Notification.Application/PantryCloud.Notification.Application.csproj", "src/Services/PantryCloud.Notification/PantryCloud.Notification.Application/"]
COPY ["src/Services/PantryCloud.Notification/PantryCloud.Notification.Core/PantryCloud.Notification.Core.csproj", "src/Services/PantryCloud.Notification/PantryCloud.Notification.Core/"]
COPY ["src/Services/PantryCloud.Notification/PantryCloud.Notification.Infrastructure/PantryCloud.Notification.Infrastructure.csproj", "src/Services/PantryCloud.Notification/PantryCloud.Notification.Infrastructure/"]
COPY ["src/Services/PantryCloud.Household/PantryCloud.Household.Application/PantryCloud.Household.Application.csproj", "src/Services/PantryCloud.Household/PantryCloud.Household.Application/"]

RUN dotnet restore "src/Services/PantryCloud.Notification/PantryCloud.Notification.Presentation/PantryCloud.Notification.Presentation.csproj"

COPY . .

WORKDIR "/src/src/Services/PantryCloud.Notification/PantryCloud.Notification.Presentation"
RUN dotnet build "./PantryCloud.Notification.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PantryCloud.Notification.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PantryCloud.Notification.Presentation.dll"]

