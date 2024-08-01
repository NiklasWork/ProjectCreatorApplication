FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app
COPY ProjectCreatorApplication.csproj ./
RUN dotnet restore
COPY . ./
RUN dotnet build -o out

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .
EXPOSE 80
ENTRYPOINT ["dotnet", "ProjectCreatorApplication.dll"]
