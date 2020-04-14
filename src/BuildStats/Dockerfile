FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.fsproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o published

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine AS runtime
WORKDIR /app
COPY --from=build /app/published .
ENV ASPNETCORE_URLS http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "BuildStats.dll"]