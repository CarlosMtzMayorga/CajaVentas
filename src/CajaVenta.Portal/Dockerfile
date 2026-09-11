# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY CajaVenta.sln .
COPY src/CajaVenta.Domain/CajaVenta.Domain.csproj src/CajaVenta.Domain/
COPY src/CajaVenta.Application/CajaVenta.Application.csproj src/CajaVenta.Application/
COPY src/CajaVenta.Infrastructure/CajaVenta.Infrastructure.csproj src/CajaVenta.Infrastructure/
COPY src/CajaVenta.Portal/CajaVenta.Portal.csproj src/CajaVenta.Portal/

# Restore dependencies
RUN dotnet restore CajaVenta.sln

# Copy source code
COPY src/ src/

# Build and publish
RUN dotnet publish src/CajaVenta.Portal/CajaVenta.Portal.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create non-root user
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

# Copy published app
COPY --from=build /app/publish .

# Create data directory for SQLite databases
RUN mkdir -p /app/data/clientes && chown -R appuser /app/data

# Expose port
EXPOSE 8080

# Environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "CajaVenta.Portal.dll"]