FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build
WORKDIR /app

COPY . /app/
WORKDIR /app/BuildStats
RUN dotnet restore
RUN dotnet publish -c Release -o published

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine AS runtime
WORKDIR /app
COPY --from=build /app/BuildStats/published .
ENV ASPNETCORE_URLS http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "BuildStats.dll"]