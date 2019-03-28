FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["Kinoheld.Web/Kinoheld.Web.csproj", "Kinoheld.Web/"]
RUN dotnet restore "Kinoheld.Web/Kinoheld.Web.csproj"
COPY . .
WORKDIR "/src/Kinoheld.Web"
RUN dotnet build "Kinoheld.Web.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Kinoheld.Web.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Kinoheld.Web.dll"]