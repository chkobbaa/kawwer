# Build stage — runs on the Oracle VM (ARM64) during `docker compose build`.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Kawwer.Api/Kawwer.Api.csproj Kawwer.Api/
COPY Kawwer.Application/Kawwer.Application.csproj Kawwer.Application/
COPY Kawwer.Contracts/Kawwer.Contracts.csproj Kawwer.Contracts/
COPY Kawwer.Domain/Kawwer.Domain.csproj Kawwer.Domain/
COPY Kawwer.Infrastructure/Kawwer.Infrastructure.csproj Kawwer.Infrastructure/

RUN dotnet restore Kawwer.Api/Kawwer.Api.csproj

COPY Kawwer.Api/ Kawwer.Api/
COPY Kawwer.Application/ Kawwer.Application/
COPY Kawwer.Contracts/ Kawwer.Contracts/
COPY Kawwer.Domain/ Kawwer.Domain/
COPY Kawwer.Infrastructure/ Kawwer.Infrastructure/

RUN dotnet publish Kawwer.Api/Kawwer.Api.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Kawwer.Api.dll"]
