FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PaymentGateway.sln", "./"]
COPY ["src", "./src"]
COPY ["tests", "./tests"]
RUN dotnet restore "PaymentGateway.sln"
RUN dotnet publish "src/PaymentGateway.Api/PaymentGateway.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "PaymentGateway.Api.dll"] 