FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
WORKDIR /src
COPY  ["kafka_dotnet.Shared/kafka_dotnet.Shared.csproj",                           "kafka_dotnet/kafka_dotnet.Shared/"]
# COPY ["kafka_dotnet/kafka_dotnet.Infrastructure/kafka_dotnet.Infrastructure.csproj",                 "kafka_dotnet/kafka_dotnet.Infrastructure/"]
# COPY ["kafka_dotnet/kafka_dotnet.Domain/kafka_dotnet.Domain.csproj",           "kafka_dotnet/kafka_dotnet.Domain/"]
# COPY ["kafka_dotnet/kafka_dotnet.Application/kafka_dotnet.Application.csproj", "kafka_dotnet/kafka_dotnet.Application/"]
# COPY ["kafka_dotnet/kafka_dotnet.Infrastructure/kafka_dotnet.Infrastructure.csproj", "kafka_dotnet/kafka_dotnet.Infrastructure/"]
COPY ["kafka_dotnet.API/kafka_dotnet.API.csproj", "kafka_dotnet/kafka_dotnet.API/"]
RUN dotnet restore "kafka_dotnet/kafka_dotnet.API/kafka_dotnet.API.csproj"

FROM restore AS build
COPY . .
RUN dotnet build "kafka_dotnet.API/kafka_dotnet.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "kafka_dotnet.API/kafka_dotnet.API.csproj" -c Release -o /app/publish /p:UseAppHost=false
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
#RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
WORKDIR /app
COPY --from=publish --chown=appuser:appgroup /app/publish .
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:8080/health/live || exit 1
#USER appuser
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["dotnet", "kafka_dotnet.API.dll"]