# Sử dụng image SDK để build code
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# SỬA Ở ĐÂY: Trỏ đúng vào thư mục con Set_BE
COPY ["Set_BE/Set_BE.csproj", "Set_BE/"]
RUN dotnet restore "Set_BE/Set_BE.csproj"

# Copy toàn bộ code còn lại vào
COPY . .

# SỬA Ở ĐÂY: Di chuyển hẳn vào thư mục con để build
WORKDIR "/src/Set_BE"
RUN dotnet publish "Set_BE.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Sử dụng image ASP.NET Runtime để chạy app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

# Chép code đã build từ bước trên sang
COPY --from=build /app/publish .

# Lệnh khởi chạy server
ENTRYPOINT ["dotnet", "Set_BE.dll"]