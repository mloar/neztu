.SUFFIXES: .cpp

all: neztu.fcgi neztud tagreader

neztu.fcgi: main.o Configuration.o Database.o FCgiIO.o Dispatcher.o Request.o Scheduler.o
	g++ -Wall -Werror -pedantic -g -o $@ $^ -lpqxx -lfcgi++ -lcgicc

neztud: Daemon.o Configuration.o Database.o Scheduler.o
	g++ -Wall -Werror -pedantic -g -o $@ $^ -lpqxx

tagreader: TagReader.o Configuration.o Database.o
	g++ -Wall -Werror -pedantic -g -o $@ $^ -lpqxx -ltag

.cpp.o:
	g++ -Wall -Werror -pedantic -g -c -I/usr/include -I/usr/include/taglib $<

clean:
	-rm *.o neztud tagreader neztu.fcgi
