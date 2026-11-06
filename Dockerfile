# syntax=docker/dockerfile:1
# Cross-build for the Pi: docker buildx build --platform linux/arm64 -t menuplanner:latest --load .
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG TARGETARCH
WORKDIR /src
COPY MenuPlanner/MenuPlanner.csproj MenuPlanner/
RUN dotnet restore MenuPlanner/MenuPlanner.csproj -a $TARGETARCH
COPY MenuPlanner/ MenuPlanner/
# No --no-restore: blazor.web.js is only added when the SDK sees the .razor sources.
RUN dotnet publish MenuPlanner/MenuPlanner.csproj -c Release -a $TARGETARCH -o /app \
 && ls /app/wwwroot/_framework/blazor.web*.js > /dev/null \
 && rm -f /app/menuplanner.db*

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
RUN mkdir -p /data && chown $APP_UID /data
ENV ASPNETCORE_HTTP_PORTS=8080 \
    MenuPlanner__DataDirectory=/data
VOLUME /data
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "MenuPlanner.dll"]
