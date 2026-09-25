FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["PlayniteAnywhere.Backend.csproj", "."]
RUN dotnet restore "PlayniteAnywhere.Backend.csproj"

COPY . .
RUN dotnet publish "PlayniteAnywhere.Backend.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

FROM runtime AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "PlayniteAnywhere.Backend.dll"]