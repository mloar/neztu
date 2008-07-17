all: Neztu.dll TagReader.exe Daemon.exe

Neztu.dll: *.cs libs/taglib-sharp/src/taglib-sharp.dll
	MONO_PATH=libs/taglib-sharp/src gmcs -out:$@ -debug:full -t:library -r:System.Configuration -r:System.Data -r:System.Web -r:Npgsql -r:taglib-sharp *.cs

TagReader.exe: TagReader.cs Database.cs PostgresBackend.cs
	MONO_PATH=libs/taglib-sharp/src gmcs -out:$@ -t:exe -debug:full -r:System.Configuration -r:System.Data -r:Npgsql -r:taglib-sharp $^

Daemon.exe: Daemon.cs Database.cs PostgresBackend.cs Scheduler.cs
	gmcs -out:$@ -t:exe -debug:full -r:System.Configuration -r:System.Data -r:Npgsql $^

libs/PgProviders/PostgreSQLProviders.dll: libs/PgProviders/Properties/Resources.resources libs/PgProviders/*.cs libs/PgProviders/Properties/*.cs
	-patch -p0 -N -s < PgProviders.patch
	gmcs -target:library -out:$@ -debug:full -r:Npgsql -r:System.Configuration -r:System.Data -r:System.Web -resource:libs/PgProviders/Properties/Resources.resources,NauckIT.PostgreSQLProvider.Properties.Resources.resources libs/PgProviders/*.cs libs/PgProviders/Properties/*.cs

libs/PgProviders/Properties/Resources.resources: libs/PgProviders/Properties/*.resx
	resgen /compile $^

libs/taglib-sharp/src/taglib-sharp.dll: libs/taglib-sharp/Makefile
	cd libs/taglib-sharp && make

libs/taglib-sharp/Makefile:
	cd libs/taglib-sharp && ./autogen.sh --disable-docs

install: Neztu.dll libs/taglib-sharp/src/taglib-sharp.dll libs/PgProviders/PostgreSQLProviders.dll Daemon.exe TagReader.exe
	install Neztu.dll ../www/bin
	install TagReader.exe ../www/bin
	install TagReader.exe.config ../www/bin
	install Daemon.exe ../www/bin
	install Daemon.exe.config ../www/bin
	install libs/taglib-sharp/src/taglib-sharp.dll ../www/bin
	install libs/PgProviders/PostgreSQLProviders.dll ../www/bin
