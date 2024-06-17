# Use the .NET 8.0 SDK image for building the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Define an environment variable for building inside Docker
ENV BuildingInsideDocker=true

# Copy the project files and restore dependencies
COPY ["NandaFood_Auth/NandaFood_Auth.csproj", "NandaFood_Auth/"]
COPY ["NandaFood_Menu/NandaFood_Menu.csproj", "NandaFood_Menu/"]
COPY ["NandaFood_Order/NandaFood_Order.csproj", "NandaFood_Order/"]
RUN dotnet restore "NandaFood_Order/NandaFood_Order.csproj"

# Copy the remaining files for both projects
COPY NandaFood_Auth/ NandaFood_Auth/
COPY NandaFood_Menu/ NandaFood_Menu/
COPY NandaFood_Order/ NandaFood_Order/

# Publish NandaFood_Auth
WORKDIR /src/NandaFood_Auth
RUN dotnet publish -c Release -o /app/nandafood_auth_out

# Publish NandaFood_Menu
WORKDIR /src/NandaFood_Menu
RUN dotnet publish -c Release -o /app/nandafood_menu_out

# Publish NandaFood_Order
WORKDIR /src/NandaFood_Order
RUN dotnet publish -c Release -o /app/nandafood_order_out

# Use the ASP.NET Core runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 80

# Copy the outputs from both projects
COPY --from=build /app/nandafood_auth_out .
COPY --from=build /app/nandafood_menu_out .
COPY --from=build /app/nandafood_order_out .

# Specify the entrypoint for NandaFood_Menu
ENTRYPOINT ["dotnet", "NandaFood_Order.dll", "--server.urls", "https://+:8082"]
