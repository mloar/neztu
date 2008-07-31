all: Neztu.dll TagReader.exe Daemon.exe

Neztu.dll: *.cs
	MONO_PATH=libs gmcs -out:$@ -t:library -r:System.Configuration -r:System.Data -r:System.Web -r:Npgsql -r:taglib-sharp -r:Anthem $^

TagReader.exe: TagReader.cs Database.cs PostgresBackend.cs
	MONO_PATH=libs gmcs -out:$@ -t:exe -r:System.Configuration -r:System.Data -r:Npgsql -r:taglib-sharp $^

Daemon.exe: Daemon.cs Database.cs PostgresBackend.cs Scheduler.cs
	MONO_PATH=libs gmcs -out:$@ -t:exe -r:System.Configuration -r:System.Data -r:Npgsql $^

install: Neztu.dll Daemon.exe TagReader.exe
	install Neztu.dll ../www/bin
	install TagReader.exe ../www/bin
	install TagReader.exe.config ../www/bin
	install Daemon.exe ../www/bin
	install Daemon.exe.config ../www/bin

dist: install
	cd ../www && tar -zcf ../neztu.tar.gz *
