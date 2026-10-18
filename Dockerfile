FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Caffeine/Caffeine.csproj Caffeine/
RUN dotnet restore Caffeine/Caffeine.csproj

COPY Caffeine/ Caffeine/
RUN dotnet publish Caffeine/Caffeine.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Caffeine.dll"]
