FROM microsoft/dotnet:1.1-runtime

ADD . /app
WORKDIR /app

ENV ASPNETCORE_URLS http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "BuildStats.dll"]