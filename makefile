all:
	rm -r build
	dotnet publish -o build -c RELEASE
	
	cp -r html build/html
	cp scripts/start.sh build/

package:
	$(MAKE)
	cd build && tar c * >../GermanBread.Mate.tar
	zstd --rm -f GermanBread.Mate.tar