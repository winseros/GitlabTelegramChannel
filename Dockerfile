FROM microsoft/dotnet:2.1.500-sdk as build
WORKDIR /build

#copy the files, required for "dotnet restore"
COPY src/TGramIntegration.sln .
COPY src/Test/TGramDaemon.Test/*.csproj ./Test/TGramDaemon.Test/
COPY src/Test/TGramTestUtil/*.csproj ./Test/TGramTestUtil/
COPY src/Test/TGramWeb.Test/*.csproj ./Test/TGramWeb.Test/
COPY src/Integration/TGramWeb.Integration/*.csproj ./Integration/TGramWeb.Integration/
COPY src/TGramCommon/*.csproj ./TGramCommon/
COPY src/TGramDaemon/*.csproj ./TGramDaemon/
COPY src/TGramWeb/*.csproj ./TGramWeb/

#get the "dotnet restore" layer cached
RUN dotnet restore

#build and test the app
COPY src/ ./
RUN dotnet build -c RELEASE && \
    dotnet test -c RELEASE --no-build && \
    dotnet publish -c RELEASE TGramWeb -o /publish

#runtime container
FROM microsoft/dotnet:2.1-aspnetcore-runtime as app
WORKDIR /app
COPY --from=build /publish/ ./
ENTRYPOINT ["dotnet", "TGramWeb.dll"]
EXPOSE 5000/tcp
ENV ASPNETCORE_ENVIRONMENT Production
ENV ASPNETCORE_URLS http://localhost:5000