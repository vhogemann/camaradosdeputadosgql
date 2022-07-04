FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["CamaraGQL/CamaraGQL.fsproj", "CamaraGQL/"]
RUN dotnet restore "CamaraGQL/CamaraGQL.fsproj"
COPY . .
WORKDIR "/src/CamaraGQL"
RUN dotnet build "CamaraGQL.fsproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CamaraGQL.fsproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CamaraGQL.dll"]
