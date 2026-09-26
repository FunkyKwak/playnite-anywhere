FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["PlayniteAnywhere.Common/PlayniteAnywhere.Common.csproj", "PlayniteAnywhere.Common/"]
COPY ["PlayniteAnywhere.Backend/PlayniteAnywhere.Backend.csproj", "PlayniteAnywhere.Backend/"]

RUN dotnet restore "PlayniteAnywhere.Backend/PlayniteAnywhere.Backend.csproj"

COPY . .

RUN dotnet publish "PlayniteAnywhere.Backend/PlayniteAnywhere.Backend.csproj" \
    -c Release \
    -o /app/publish

COPY Web /app/web

FROM runtime AS final
WORKDIR /app

COPY --from=build /app/publish .
COPY --from=build /app/web ./web

ENTRYPOINT ["dotnet", "PlayniteAnywhere.Backend.dll"]