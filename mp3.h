#ifndef NEZTU_MP3_H
#define NEZTU_MP3_H

#include <string>
#include <iostream>
#include "streamer.h"
using namespace std;

#include <AL/al.h>
#include <libmpeg3.h>

#define BUFSIZE (4096 * 4)

class mp3_stream : public streamer
{
    public:
        virtual ~mp3_stream();
        static bool try_open(const string &path);
        virtual void open(const char *path);
        virtual void display();
        virtual bool playback();
        virtual bool playing();
        virtual bool update();

    protected:

        bool stream(ALuint buffer);
        void empty();
        void check();
        string errorString(int code);

    private:

        mpeg3_t*        mp3File;
        int channels;

        ALuint buffers[2];
        ALuint source;
        ALenum format;
};


#endif // NEZTU_MP3_H
