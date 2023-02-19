FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["OutlayApp.API/OutlayApp.API.csproj", "OutlayApp.API/"]
COPY ["OutlayApp.Application/OutlayApp.Application.csproj", "OutlayApp.Application/"]
COPY ["OutlayApp.Domain/OutlayApp.Domain.csproj", "OutlayApp.Domain/"]
COPY ["OutlayApp.Infrastructure/OutlayApp.Infrastructure.csproj", "OutlayApp.Infrastructure/"]
RUN dotnet restore "OutlayApp.API/OutlayApp.API.csproj"
COPY . .
WORKDIR "/src/OutlayApp.API"
RUN dotnet build "OutlayApp.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OutlayApp.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OutlayApp.API.dll"]
