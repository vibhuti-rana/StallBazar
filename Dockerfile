FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY StallBazar.csproj ./
RUN dotnet restore StallBazar.csproj

COPY . ./
RUN dotnet publish StallBazar.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production

RUN mkdir -p /data

COPY --from=build /app/publish ./

ENTRYPOINT ["dotnet", "StallBazar.dll"]
