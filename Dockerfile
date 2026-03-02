# syntax=docker/dockerfile:1.20

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

RUN dotnet tool install --global dotnet-ef --version 8.0.22
ENV PATH="$PATH:/root/.dotnet/tools"

COPY --parents src/**/*.csproj .
RUN dotnet restore src/Mechanics.Api/Mechanics.Api.csproj --locked-mode

COPY src src
RUN dotnet build src/Mechanics.Api/Mechanics.Api.csproj --no-restore

RUN dotnet ef migrations bundle --project src/Mechanics.Infra.Data --startup-project src/Mechanics.Api --no-build --output /dist/efbundle
RUN dotnet publish src/Mechanics.Api/Mechanics.Api.csproj --no-restore -c Release -o /dist

FROM mcr.microsoft.com/dotnet/aspnet:8.0-noble-chiseled-extra AS final

ENV LANG=pt_BR.UTF-8 LANGUAGE=pt_BR:pt LC_ALL=pt_BR.UTF-8
EXPOSE 8080

WORKDIR /app
COPY --from=build /dist .

ENTRYPOINT ["dotnet", "Mechanics.Api.dll"]
