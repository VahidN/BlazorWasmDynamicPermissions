dotnet tool update --global dotnet-ef --version 6.0.5
dotnet build
dotnet ef database update
pause