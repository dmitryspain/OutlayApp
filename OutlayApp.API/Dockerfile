# Keep in step with <TargetFramework> in the csproj files (net8.0 -> 8.0).
ARG DOTNET_VERSION=8.0

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src
COPY ["OutlayApp.API/OutlayApp.API.csproj", "OutlayApp.API/"]
COPY ["OutlayApp.Application/OutlayApp.Application.csproj", "OutlayApp.Application/"]
COPY ["OutlayApp.Domain/OutlayApp.Domain.csproj", "OutlayApp.Domain/"]
COPY ["OutlayApp.Infrastructure/OutlayApp.Infrastructure.csproj", "OutlayApp.Infrastructure/"]
RUN dotnet restore "OutlayApp.API/OutlayApp.API.csproj"
COPY . .
RUN dotnet publish "OutlayApp.API/OutlayApp.API.csproj" -c Release -o /app/publish --no-restore -p:UseAppHost=false

# Chiseled: no shell or package manager, so far fewer OS packages to patch.
# "-extra" brings tzdata (KyivTime needs Europe/Kyiv) and ICU.
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION}-noble-chiseled-extra AS final
WORKDIR /app
COPY --from=build /app/publish .
# Non-root user shipped with the .NET images; it can't bind ports below 1024, hence 8080.
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "OutlayApp.API.dll"]
