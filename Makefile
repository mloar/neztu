.SUFFIXES: .cpp

all: neztu.fcgi tagreader

neztu.fcgi: main.o Database.o FCgiIO.o Dispatcher.o
	g++ -Wall -Werror -pedantic -g -o $@ $^ -lpqxx -lfcgi++ -lcgicc

tagreader: TagReader.o Database.o
	g++ -Wall -Werror -pedantic -g -o $@ $^ -lpqxx -ltag

.cpp.o:
	g++ -Wall -Werror -pedantic -g -c -I/usr/include -I/usr/include/taglib $<
