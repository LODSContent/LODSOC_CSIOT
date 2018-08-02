FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/aspnetcore-build:2.0 AS build
WORKDIR /src
COPY deviceSimulator/deviceSimulator.csproj deviceSimulator/
RUN dotnet restore deviceSimulator/deviceSimulator.csproj
COPY . .
WORKDIR /src/deviceSimulator
RUN dotnet build deviceSimulator.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish deviceSimulator.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "deviceSimulator.dll"]
