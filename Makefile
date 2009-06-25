.SUFFIXES: .c .cpp

DEFINES='-DSYSCONFDIR="/etc"' -D_FILE_OFFSET_BITS=64

all: neztu.fcgi neztud tagreader

neztu.fcgi: main.o Configuration.o Database.o Dispatcher.o FCgiIO.o Request.o Scheduler.o TagReader.o
	g++ -Wall -Werror -pedantic -g -o $@ $^ -lpqxx -lfcgi++ -lcgicc -lboost_filesystem-mt -ltag

neztud: Daemon.o Configuration.o Database.o Scheduler.o Player.o MP3Decoder.o OggVorbisDecoder.o
	g++ -Wall -Werror -pedantic -g -o $@ $^ -lopenal -lmpg123 -logg -lvorbis -lvorbisfile -lalut -lpqxx

tagreader: TagReader.o Configuration.o Database.o TagReaderMain.o
	g++ -Wall -Werror -pedantic -g -o $@ $^ -lboost_filesystem-mt -lpqxx -ltag

.cpp.o:
	g++ -Wall -Werror -pedantic $(DEFINES) -g -c -I/usr/include -I/usr/include/taglib $<

.c.o:
	gcc -Wall -Werror $(DEFINES) -g -c -I/usr/include -I/usr/include/taglib $<

clean:
	-rm *.o neztud tagreader neztu.fcgi
