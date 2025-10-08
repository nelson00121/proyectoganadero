# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar archivos de proyecto
COPY ["BackOffice/BackOffice.csproj", "BackOffice/"]
COPY ["Database/Database.csproj", "Database/"]
COPY ["Bucket/Bucket.csproj", "Bucket/"]
COPY ["Bycript/Bycript.csproj", "Bycript/"]

# Restaurar dependencias
RUN dotnet restore "BackOffice/BackOffice.csproj"

# Copiar todo el código
COPY . .

# Build
WORKDIR "/src/BackOffice"
RUN dotnet build "BackOffice.csproj" -c Release -o /app/build

# Etapa 2: Publish
FROM build AS publish
RUN dotnet publish "BackOffice.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copiar archivos publicados
COPY --from=publish /app/publish .

# Exponer puerto
EXPOSE 80
EXPOSE 443

# Punto de entrada
ENTRYPOINT ["dotnet", "BackOffice.dll"]
