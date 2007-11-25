Neztu.dll: *.cs libs/taglib-sharp/src/taglib-sharp.dll
	MONO_PATH=libs/taglib-sharp/src gmcs -out:$@ -t:library -r:System.Configuration -r:System.Data -r:System.Web -r:Npgsql -r:taglib-sharp *.cs

TagReader.exe: TagReader.cs Database.cs PostgresBackend.cs
	gmcs -out:$@ -t:exe -debug:full -r:System.Configuration -r:Npgsql -r:taglib-sharp $^

libs/PgProviders/PostgreSQLProviders.dll: libs/PgProviders/Properties/Resources.resources libs/PgProviders/*.cs libs/PgProviders/Properties/*.cs
	-patch -p0 -N -s < PgProviders.patch
	gmcs -target:library -out:$@ -r:Npgsql -r:System.Configuration -r:System.Data -r:System.Web -resource:libs/PgProviders/Properties/Resources.resources,NauckIT.PostgreSQLProvider.Properties.Resources.resources libs/PgProviders/*.cs libs/PgProviders/Properties/*.cs

libs/PgProviders/Properties/Resources.resources: libs/PgProviders/Properties/*.resx
	resgen2 /compile $^

libs/taglib-sharp/src/taglib-sharp.dll: libs/taglib-sharp/Makefile
	cd libs/taglib-sharp && make

libs/taglib-sharp/Makefile:
	cd libs/taglib-sharp && ./autogen.sh

install: Neztu.dll libs/taglib-sharp/src/taglib-sharp.dll libs/PgProviders/PostgreSQLProviders.dll
	install Neztu.dll ../www/bin
	install libs/taglib-sharp/src/taglib-sharp.dll ../www/bin
	install libs/PgProviders/PostgreSQLProviders.dll ../www/bin
