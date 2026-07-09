FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj and restore as distinct layers
COPY *.sln .
COPY Backend_Mboagreen.csproj .
RUN dotnet restore Backend_Mboagreen.csproj

# copy everything and build
COPY . .
RUN dotnet publish Backend_Mboagreen.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Create a non-root user
RUN useradd -m -u 1001 appuser

WORKDIR /app
COPY --from=build --chown=appuser:appuser /app/publish ./

# Set security headers and environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

EXPOSE 8080

# Run as non-root user for security
USER appuser

ENTRYPOINT ["dotnet", "Backend_Mboagreen.dll"]
