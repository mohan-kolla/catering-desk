# Builds and runs the C# API. Build from the backend folder:
#   docker build -t catering-demo-api .
#   docker run -p 8080:8080 catering-demo-api
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY CateringDemo.Api/CateringDemo.Api.csproj CateringDemo.Api/
RUN dotnet restore CateringDemo.Api/CateringDemo.Api.csproj
COPY CateringDemo.Api/ CateringDemo.Api/
RUN dotnet publish CateringDemo.Api/CateringDemo.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
# The .NET image runs as a non-root user, so keep the SQLite file in a writable folder.
ENV ConnectionStrings__Default="Data Source=/tmp/catering-demo.db"
EXPOSE 8080
ENTRYPOINT ["dotnet", "CateringDemo.Api.dll"]
