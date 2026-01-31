FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY src/BuildingBlocks/PantryCloud.SharedKernel/PantryCloud.SharedKernel.csproj src/BuildingBlocks/PantryCloud.SharedKernel/
COPY src/Services/PantryCloud.Recipe/PantryCloud.Recipe.Presentation/PantryCloud.Recipe.Presentation.csproj src/Services/PantryCloud.Recipe/PantryCloud.Recipe.Presentation/
COPY src/Services/PantryCloud.Recipe/PantryCloud.Recipe.Application/PantryCloud.Recipe.Application.csproj src/Services/PantryCloud.Recipe/PantryCloud.Recipe.Application/
COPY src/Services/PantryCloud.Recipe/PantryCloud.Recipe.Infrastructure/PantryCloud.Recipe.Infrastructure.csproj src/Services/PantryCloud.Recipe/PantryCloud.Recipe.Infrastructure/
COPY src/Services/PantryCloud.Recipe/PantryCloud.Recipe.Core/PantryCloud.Recipe.Core.csproj src/Services/PantryCloud.Recipe/PantryCloud.Recipe.Core/

# Restore dependencies
RUN dotnet restore "src/Services/PantryCloud.Recipe/PantryCloud.Recipe.Presentation/PantryCloud.Recipe.Presentation.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR /src/src/Services/PantryCloud.Recipe/PantryCloud.Recipe.Presentation
RUN dotnet build "./PantryCloud.Recipe.Presentation.csproj" -c Release -o /app/build

# Publish
RUN dotnet publish "./PantryCloud.Recipe.Presentation.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PantryCloud.Recipe.Presentation.dll"]


