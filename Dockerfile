# See https://aka.ms/customizecontainer to learn how to customize your container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# API stages
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS api-base
USER root
RUN mkdir -p /data && chown $APP_UID /data
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS api-build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["UserDirectory.API/UserDirectory.API.csproj", "UserDirectory.API/"]
COPY ["UserDirectory.Application/UserDirectory.Application.csproj", "UserDirectory.Application/"]
COPY ["UserDirectory.Domain/UserDirectory.Domain.csproj", "UserDirectory.Domain/"]
COPY ["UserDirectory.Infrastructure/UserDirectory.Infrastructure.csproj", "UserDirectory.Infrastructure/"]
RUN dotnet restore "./UserDirectory.API/UserDirectory.API.csproj"
COPY . .
WORKDIR "/src/UserDirectory.API"
RUN dotnet build "./UserDirectory.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM api-build AS api-publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./UserDirectory.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM api-base AS api
WORKDIR /app
COPY --from=api-publish /app/publish .
ENTRYPOINT ["dotnet", "UserDirectory.API.dll"]

# React frontend stage
FROM node:20-alpine AS web
WORKDIR /src/UserDirectory.Web
COPY ["UserDirectory.Web/package.json", "./"]
RUN npm install
COPY UserDirectory.Web/ ./
ARG VITE_API_URL=http://localhost:8080
ARG VITE_KEYCLOAK_URL=http://localhost:8082
ARG VITE_KEYCLOAK_REALM=user-directory
ARG VITE_KEYCLOAK_CLIENT_ID=user-directory-frontend
ENV VITE_API_URL=$VITE_API_URL
ENV VITE_KEYCLOAK_URL=$VITE_KEYCLOAK_URL
ENV VITE_KEYCLOAK_REALM=$VITE_KEYCLOAK_REALM
ENV VITE_KEYCLOAK_CLIENT_ID=$VITE_KEYCLOAK_CLIENT_ID
EXPOSE 5173
CMD ["npm", "run", "dev", "--", "--host", "0.0.0.0"]