FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files
COPY ["src/BuildingBlocks/PantryCloud.SharedKernel/PantryCloud.SharedKernel.csproj", "src/BuildingBlocks/PantryCloud.SharedKernel/"]
COPY ["src/Services/PantryCloud.Identity/PantryCloud.Identity.Presentation/PantryCloud.Identity.Presentation.csproj", "src/Services/PantryCloud.Identity/PantryCloud.Identity.Presentation/"]
COPY ["src/Services/PantryCloud.Identity/PantryCloud.Identity.Application/PantryCloud.Identity.Application.csproj", "src/Services/PantryCloud.Identity/PantryCloud.Identity.Application/"]
COPY ["src/Services/PantryCloud.Identity/PantryCloud.Identity.Core/PantryCloud.Identity.Core.csproj", "src/Services/PantryCloud.Identity/PantryCloud.Identity.Core/"]
COPY ["src/Services/PantryCloud.Identity/PantryCloud.Identity.Infrastructure/PantryCloud.Identity.Infrastructure.csproj", "src/Services/PantryCloud.Identity/PantryCloud.Identity.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/Services/PantryCloud.Identity/PantryCloud.Identity.Presentation/PantryCloud.Identity.Presentation.csproj"

# Workaround for "layer does not exist" during export
RUN true

# Copy rest of the code
COPY . .

# Build
WORKDIR "/src/src/Services/PantryCloud.Identity/PantryCloud.Identity.Presentation"
RUN dotnet build "./PantryCloud.Identity.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PantryCloud.Identity.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
# Workaround for "layer does not exist" during export
RUN true
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PantryCloud.Identity.Presentation.dll"]
