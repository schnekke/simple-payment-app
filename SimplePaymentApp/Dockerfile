FROM microsoft/aspnetcore-build as build-env  
WORKDIR /source  
COPY . .  
RUN dotnet restore  
RUN dotnet publish -o /publish --configuration Release  
  
# Publish Stage  
FROM microsoft/aspnetcore  
WORKDIR /app  
COPY --from=build-env /publish .  
ENTRYPOINT ["dotnet", "SimplePaymentApp.dll"]  