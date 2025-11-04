# ---------- build stage ----------
FROM ubuntu:22.04 AS build

# Instalar dependencias básicas y el SDK de .NET 8.0
RUN apt-get update && apt-get install -y wget apt-transport-https software-properties-common \
    && wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb \
    && dpkg -i packages-microsoft-prod.deb \
    && apt-get update && apt-get install -y dotnet-sdk-8.0

WORKDIR /src

# Copiar la solución principal
COPY taller.sln ./

# Copiar los proyectos
COPY Web/Web.csproj Web/
COPY Business/Business.csproj Business/
COPY Data/Data.csproj Data/
COPY Entity/Entity.csproj Entity/
COPY Helpers/Helpers.csproj Helpers/
COPY Utilities/Utilities.csproj Utilities/

# Restaurar dependencias
RUN dotnet restore Web/Web.csproj

# Copiar el resto del código fuente
COPY . .

# Compilar y publicar el proyecto principal en modo Release
RUN dotnet publish Web/Web.csproj -c Release -o /app/publish /p:UseAppHost=false


# ---------- runtime stage ----------
FROM ubuntu:22.04 AS final

# Instalar runtime de .NET 8.0 y configurar zona horaria
RUN apt-get update && \
    apt-get install -y wget apt-transport-https software-properties-common tzdata && \
    ln -fs /usr/share/zoneinfo/America/Bogota /etc/localtime && \
    dpkg-reconfigure -f noninteractive tzdata && \
    wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \
    apt-get update && apt-get install -y aspnetcore-runtime-8.0 && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /app

# Variables de entorno
ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    TZ=America/Bogota

# Copiar los archivos publicados desde la fase anterior
COPY --from=build /app/publish .

# Exponer el puerto del contenedor
EXPOSE 8080

# Punto de entrada
ENTRYPOINT ["dotnet", "Web.dll"]
