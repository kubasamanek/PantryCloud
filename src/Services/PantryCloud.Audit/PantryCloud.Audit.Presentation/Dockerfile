FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files
COPY ["src/BuildingBlocks/PantryCloud.SharedKernel/PantryCloud.SharedKernel.csproj", "src/BuildingBlocks/PantryCloud.SharedKernel/"]
COPY ["src/Services/PantryCloud.Audit/PantryCloud.Audit.Presentation/PantryCloud.Audit.Presentation.csproj", "src/Services/PantryCloud.Audit/PantryCloud.Audit.Presentation/"]
COPY ["src/Services/PantryCloud.Audit/PantryCloud.Audit.Application/PantryCloud.Audit.Application.csproj", "src/Services/PantryCloud.Audit/PantryCloud.Audit.Application/"]
COPY ["src/Services/PantryCloud.Audit/PantryCloud.Audit.Core/PantryCloud.Audit.Core.csproj", "src/Services/PantryCloud.Audit/PantryCloud.Audit.Core/"]
COPY ["src/Services/PantryCloud.Audit/PantryCloud.Audit.Infrastructure/PantryCloud.Audit.Infrastructure.csproj", "src/Services/PantryCloud.Audit/PantryCloud.Audit.Infrastructure/"]
COPY ["src/Services/PantryCloud.Household/PantryCloud.Household.Application/PantryCloud.Household.Application.csproj", "src/Services/PantryCloud.Household/PantryCloud.Household.Application/"]
COPY ["src/Services/PantryCloud.Pantry/PantryCloud.Pantry.Application/PantryCloud.Pantry.Application.csproj", "src/Services/PantryCloud.Pantry/PantryCloud.Pantry.Application/"]
COPY ["src/Services/PantryCloud.ShoppingList/PantryCloud.ShoppingList.Application/PantryCloud.ShoppingList.Application.csproj", "src/Services/PantryCloud.ShoppingList/PantryCloud.ShoppingList.Application/"]

# Restore dependencies
RUN dotnet restore "src/Services/PantryCloud.Audit/PantryCloud.Audit.Presentation/PantryCloud.Audit.Presentation.csproj"

# Copy rest of the code
COPY . .

# Build
WORKDIR "/src/src/Services/PantryCloud.Audit/PantryCloud.Audit.Presentation"
RUN dotnet build "./PantryCloud.Audit.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PantryCloud.Audit.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN chown -R app:app /app
USER app
ENTRYPOINT ["dotnet", "PantryCloud.Audit.Presentation.dll"]
