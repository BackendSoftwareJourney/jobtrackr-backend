FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/JobTrackr.Api/JobTrackr.Api.csproj", "src/JobTrackr.Api/"]
COPY ["src/JobTrackr.Application/JobTrackr.Application.csproj", "src/JobTrackr.Application/"]
COPY ["src/JobTrackr.Domain/JobTrackr.Domain.csproj", "src/JobTrackr.Domain/"]
COPY ["src/JobTrackr.Infrastructure/JobTrackr.Infrastructure.csproj", "src/JobTrackr.Infrastructure/"]
RUN dotnet restore src/JobTrackr.Api/JobTrackr.Api.csproj

COPY src/ ./src/
RUN dotnet publish src/JobTrackr.Api/JobTrackr.Api.csproj \
    --configuration Release --output /out --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /out/ ./
USER app
ENTRYPOINT ["dotnet", "JobTrackr.Api.dll"]