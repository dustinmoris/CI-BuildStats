FROM microsoft/dotnet:2.1.5-aspnetcore-runtime

ADD . /app
WORKDIR /app

ENV ASPNETCORE_URLS http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "BuildStats.dll"]