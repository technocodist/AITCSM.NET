default: run

build: AITCSM.NET
	dotnet build

run: AITCSM.NET
	dotnet run --project .\AITCSM.NET\AITCSM.NET.csproj

format:
	dotnet format .\AITCSM.NET.sln whitespace
	dotnet format .\AITCSM.NET.sln style
	dotnet format .\AITCSM.NET.sln analyzers

clean:
	rm -rf Results