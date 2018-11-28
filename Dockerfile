FROM microsoft/dotnet:2.1.500-sdk as build
COPY src/ .
RUN dotnet restore && \
    dotnet build -c RELEASE && \
    dotnet test -c RELEASE && \
    dotnet publish -c RELEASE TGramWeb -o ./publish

FROM microsoft/dotnet:2.1-aspnetcore-runtime as app
COPY --from=build TGramWeb/publish/ /app
WORKDIR /app
ENTRYPOINT ["dotnet", "TGramWeb.dll"]
EXPOSE 5000/tcp
ENV ASPNETCORE_ENVIRONMENT Production
ENV ASPNETCORE_URLS http://localhost:5000