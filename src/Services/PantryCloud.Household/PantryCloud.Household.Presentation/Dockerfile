FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files
COPY ["src/BuildingBlocks/PantryCloud.SharedKernel/PantryCloud.SharedKernel.csproj", "src/BuildingBlocks/PantryCloud.SharedKernel/"]
COPY ["src/Services/PantryCloud.Household/PantryCloud.Household.Presentation/PantryCloud.Household.Presentation.csproj", "src/Services/PantryCloud.Household/PantryCloud.Household.Presentation/"]
COPY ["src/Services/PantryCloud.Household/PantryCloud.Household.Application/PantryCloud.Household.Application.csproj", "src/Services/PantryCloud.Household/PantryCloud.Household.Application/"]
COPY ["src/Services/PantryCloud.Household/PantryCloud.Household.Core/PantryCloud.Household.Core.csproj", "src/Services/PantryCloud.Household/PantryCloud.Household.Core/"]
COPY ["src/BuildingBlocks/PantryCloud.SharedKernel/PantryCloud.SharedKernel.csproj", "src/BuildingBlocks/PantryCloud.SharedKernel/"]
COPY ["src/Services/PantryCloud.Household/PantryCloud.Household.Infrastructure/PantryCloud.Household.Infrastructure.csproj", "src/Services/PantryCloud.Household/PantryCloud.Household.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/Services/PantryCloud.Household/PantryCloud.Household.Presentation/PantryCloud.Household.Presentation.csproj"

# Copy rest of the code
COPY . .

# Build
WORKDIR "/src/src/Services/PantryCloud.Household/PantryCloud.Household.Presentation"
RUN dotnet build "./PantryCloud.Household.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PantryCloud.Household.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PantryCloud.Household.Presentation.dll"]
