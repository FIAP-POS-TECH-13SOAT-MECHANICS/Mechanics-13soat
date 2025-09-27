FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
COPY src .
RUN dotnet restore Mechanics.Api/Mechanics.Api.csproj
RUN dotnet publish Mechanics.Api/Mechanics.Api.csproj --no-restore -c Release -o /dist

FROM base AS final
WORKDIR /dist
COPY --from=build /dist .

ENTRYPOINT ["dotnet", "Mechanics.Api.dll"]
