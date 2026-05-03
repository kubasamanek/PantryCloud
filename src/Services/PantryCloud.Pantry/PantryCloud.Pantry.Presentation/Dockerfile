FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files
COPY ["src/BuildingBlocks/PantryCloud.SharedKernel/PantryCloud.SharedKernel.csproj", "src/BuildingBlocks/PantryCloud.SharedKernel/"]
COPY ["src/Services/PantryCloud.Pantry/PantryCloud.Pantry.Presentation/PantryCloud.Pantry.Presentation.csproj", "src/Services/PantryCloud.Pantry/PantryCloud.Pantry.Presentation/"]
COPY ["src/Services/PantryCloud.Pantry/PantryCloud.Pantry.Application/PantryCloud.Pantry.Application.csproj", "src/Services/PantryCloud.Pantry/PantryCloud.Pantry.Application/"]
COPY ["src/Services/PantryCloud.Pantry/PantryCloud.Pantry.Core/PantryCloud.Pantry.Core.csproj", "src/Services/PantryCloud.Pantry/PantryCloud.Pantry.Core/"]
COPY ["src/Services/PantryCloud.Pantry/PantryCloud.Pantry.Infrastructure/PantryCloud.Pantry.Infrastructure.csproj", "src/Services/PantryCloud.Pantry/PantryCloud.Pantry.Infrastructure/"]
COPY ["src/Services/PantryCloud.Household/PantryCloud.Household.Application/PantryCloud.Household.Application.csproj", "src/Services/PantryCloud.Household/PantryCloud.Household.Application/"]

# Restore dependencies
RUN dotnet restore "src/Services/PantryCloud.Pantry/PantryCloud.Pantry.Presentation/PantryCloud.Pantry.Presentation.csproj"

# Copy rest of the code
COPY . .

# Build
WORKDIR "/src/src/Services/PantryCloud.Pantry/PantryCloud.Pantry.Presentation"
RUN dotnet build "./PantryCloud.Pantry.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PantryCloud.Pantry.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PantryCloud.Pantry.Presentation.dll"]

