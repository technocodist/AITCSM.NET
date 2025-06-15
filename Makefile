default: run

build: AITCSM.NET
	dotnet build

run: AITCSM.NET
	dotnet run --project .\AITCSM.NET\AITCSM.NET.csproj