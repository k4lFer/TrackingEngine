FROM mcr.microsoft.com/dotnet/sdk:10.0 AS publish
WORKDIR /src

COPY ["WebApi/WebApi.csproj", "WebApi/"]
COPY ["App.Infrastructure/App.Infrastructure.csproj", "App.Infrastructure/"]
COPY ["App.Interfaces/App.Interfaces.csproj", "App.Interfaces/"]
COPY ["App.UseCases/App.UseCases.csproj", "App.UseCases/"]
COPY ["App.Shared/App.Shared.csproj", "App.Shared/"]
COPY ["App.Domain/App.Domain.csproj", "App.Domain/"]
COPY ["App.Objects/App.Objects.csproj", "App.Objects/"]
RUN dotnet restore "WebApi/WebApi.csproj"

COPY App.Infrastructure/ App.Infrastructure/
COPY App.Interfaces/ App.Interfaces/
COPY App.UseCases/ App.UseCases/
COPY App.Shared/ App.Shared/
COPY App.Domain/ App.Domain/
COPY App.Objects/ App.Objects/
COPY WebApi/ WebApi/
WORKDIR "/src/WebApi"
RUN dotnet publish "WebApi.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
RUN apt-get update && apt-get install -y --no-install-recommends libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
COPY --from=publish /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Docker
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "WebApi.dll"]