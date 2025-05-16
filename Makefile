MAKEFLAGS += --silent

restore:
	dotnet restore --interactive
build:
	dotnet build
run:
	dotnet run