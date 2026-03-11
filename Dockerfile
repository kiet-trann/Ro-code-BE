# Sử dụng image SDK để build code
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy file .csproj và restore các thư viện
COPY ["Set_BE.csproj", "./"]
RUN dotnet restore "Set_BE.csproj"

# Copy toàn bộ code còn lại và tiến hành Build
COPY . .
RUN dotnet publish "Set_BE.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Sử dụng image ASP.NET Runtime để chạy app (nhẹ hơn)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

# Chép code đã build từ bước trên sang
COPY --from=build /app/publish .

# Lệnh khởi chạy server
ENTRYPOINT ["dotnet", "Set_BE.dll"]