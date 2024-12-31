dotnet tool update --global dotnet-ef --version 9.0.0
dotnet build
dotnet ef database update
pause