FROM microsoft/dotnet:2.1.500-sdk as build
COPY /src /src
RUN dotnet clean src/ && dotnet test -c RELEASE src/
RUN dotnet clean src/ && dotnet publish -c RELEASE src/TGramWeb

FROM microsoft/dotnet:2.1-runtime
COPY src/TGramWeb/bin/Release/netcoreapp2.1/publish/ /opt/GitlabTgramChannel/