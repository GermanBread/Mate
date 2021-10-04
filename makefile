All:
	dotnet publish -o build -c RELEASE
	cp scripts/start.sh build/