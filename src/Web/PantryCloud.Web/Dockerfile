FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/Web/PantryCloud.Web/PantryCloud.Web.csproj", "src/Web/PantryCloud.Web/"]
RUN dotnet restore "src/Web/PantryCloud.Web/PantryCloud.Web.csproj"

COPY . .
WORKDIR "/src/src/Web/PantryCloud.Web"
RUN dotnet publish "PantryCloud.Web.csproj" -c Release -o /app/publish && \
    mkdir -p /app/webroot && \
    if [ -d /app/publish/wwwroot ] && [ -f /app/publish/wwwroot/index.html ]; then \
      cp -r /app/publish/wwwroot/. /app/webroot/; \
    else \
      cp -r /app/publish/. /app/webroot/; \
    fi

FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

RUN rm -f /etc/nginx/conf.d/default.conf
COPY src/Web/PantryCloud.Web/nginx.conf /etc/nginx/conf.d/default.conf

COPY --from=build /app/webroot/. .

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
