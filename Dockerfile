FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /dist
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /dist

FROM base AS final
WORKDIR /dist
COPY --from=build /dist .

ENTRYPOINT ["dotnet", "Mechanics.Api.dll"]
