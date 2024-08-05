FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY ProjectCreatorApplication.csproj ./
RUN dotnet restore
COPY . ./
RUN dotnet build -o out

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS runtime
WORKDIR /app
RUN apt-get update && \
    apt-get install -y nodejs npm && \
    npm install -g azure-functions-core-tools@4 --unsafe-perm true && \
    apt-get install -y expect && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

COPY --from=build /app/out .
EXPOSE 80
ENTRYPOINT ["dotnet", "ProjectCreatorApplication.dll"]
