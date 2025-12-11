FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

COPY --parents src/**/*.csproj .
RUN dotnet restore src/Mechanics.Api/Mechanics.Api.csproj

COPY src src
RUN dotnet publish src/Mechanics.Api/Mechanics.Api.csproj --no-restore -c Release -o /dist

FROM mcr.microsoft.com/dotnet/aspnet:8.0-noble-chiseled-extra AS final
EXPOSE 8080

WORKDIR /dist
COPY --from=build /dist .

ENTRYPOINT ["dotnet", "Mechanics.Api.dll"]
