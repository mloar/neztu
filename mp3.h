#ifndef NEZTU_MP3_H
#define NEZTU_MP3_H

#include <string>
#include "streamer.h"
using namespace std;

#include <AL/al.h>
#include <mpg123.h>

#define BUFSIZE (4096 * 16)
#define BUFCOUNT 4

class mp3_stream : public streamer
{
    public:
        mp3_stream();
        virtual void play(const char *path);
        virtual void skip();

    protected:

        bool stream(ALuint buffer);
        void empty();
        void check();
        string errorString(int code);

    private:

        mpg123_handle*        mp3File;
        int channels;
        long rate;
        int m_skip;

        ALuint buffers[BUFCOUNT];
        ALuint source;
        ALenum format;
};


#endif // NEZTU_MP3_H
