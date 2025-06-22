default: run

build: AITCSM.NET
	dotnet build

run: AITCSM.NET
	dotnet run --project .\AITCSM.NET\AITCSM.NET.csproj

format:
	dotnet format .\AITCSM.NET.sln

clean:
	rm -rf Results