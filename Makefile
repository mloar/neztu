.SUFFIXES: .c .cpp

all: neztu.fcgi neztud tagreader

neztu.fcgi: main.o Configuration.o Database.o FCgiIO.o Dispatcher.o Request.o Scheduler.o
	g++ -Wall -Werror -pedantic -g -o $@ $^ -lpqxx -lfcgi++ -lcgicc

neztud: Daemon.o Configuration.o Database.o Scheduler.o mp3.o ogg.o
	g++ -Wall -Werror -pedantic -g -o $@ $^ -lopenal -lmpeg3 -logg -lvorbis -lvorbisfile -lalut -lpqxx

ald: ald.o
	g++ -Wall -Werror -pedantic -g -o $@ $^ -lopenal

tagreader: TagReader.o Configuration.o Database.o
	g++ -Wall -Werror -pedantic -g -o $@ $^ -lboost_filesystem -lpqxx -ltag

.cpp.o:
	g++ -Wall -Werror -pedantic -g -c -I/usr/include -I/usr/include/taglib $<

.c.o:
	gcc -Wall -Werror -g -c -I/usr/include -I/usr/include/taglib $<

clean:
	-rm *.o neztud tagreader neztu.fcgi
