.SUFFIXES: .cpp
test.fcgi: main.o Database.o FCgiIO.o Dispatcher.o
	g++ -Wall -g -o $@ $^ -lpqxx -lfcgi++ -lcgicc

.cpp.o:
	g++ -Wall -g -c -I/usr/include $<
