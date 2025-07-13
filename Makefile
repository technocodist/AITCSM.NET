default: run

build: AITCSM.NET
	dotnet build

run: AITCSM.NET
	dotnet run --project .\AITCSM.NET\AITCSM.NET.csproj

format:
	dotnet format .\AITCSM.NET.sln whitespace
	dotnet format .\AITCSM.NET.sln style
	dotnet format .\AITCSM.NET.sln analyzers

MIGRATION_NAME = "default"

migrate:
	dotnet ef migrations add $(MIGRATION_NAME) --project .\AITCSM.NET\ --output-dir Data/EF/Migrations

updatedb:
	dotnet ef database update --project .\AITCSM.NET\ --context AITCSMContext

dropdb:
	dotnet ef database drop --force --project .\AITCSM.NET\ 

clean:
	rm -rf Results