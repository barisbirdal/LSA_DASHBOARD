FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Sadece proje dosyasını kopyalayıp bağımlılıkları yüklüyoruz (Cache avantajı için)
COPY ["LSA_Dashboard.csproj", "./"]
RUN dotnet restore "LSA_Dashboard.csproj"

# Tüm dosyaları kopyalayıp projeyi yayınlıyoruz
COPY . .
RUN dotnet publish "LSA_Dashboard.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Çalışma zamanı (Runtime) imajını oluşturuyoruz
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Render'ın varsayılan olarak kullandığı veya kullanabileceği portu ayarlıyoruz
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LSA_Dashboard.dll"]
