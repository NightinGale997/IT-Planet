#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["PlanetIT/PlanetIT.csproj", "PlanetIT/"]
RUN dotnet restore "PlanetIT/PlanetIT.csproj"
COPY . .
WORKDIR "/src/PlanetIT"
RUN dotnet build "PlanetIT.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PlanetIT.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
RUN apt-get update 
RUN apt-get --yes install curl
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PlanetIT.dll"]