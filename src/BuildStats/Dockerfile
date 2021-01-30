FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build

ARG version=0.0.0-undefined

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY src/BuildStats/*.fsproj ./
RUN dotnet restore

# Copy everything else and build
COPY src/BuildStats/. ./
RUN dotnet publish /p:Version=$version -c Release -o published

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine AS runtime
WORKDIR /app
COPY --from=build /app/published .
ENTRYPOINT ["dotnet", "BuildStats.dll"]