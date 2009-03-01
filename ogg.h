#ifndef NEZTU_OGG_H
#define NEZTU_OGG_H

#include <string>
#include <iostream>
#include "streamer.h"
using namespace std;

#include <AL/al.h>
#include <ogg/ogg.h>
#include <vorbis/vorbisfile.h>



#define BUFFER_SIZE (4096 * 4)



class ogg_stream : public streamer
{
    public:
        virtual ~ogg_stream();
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

        FILE*           oggFile;
        OggVorbis_File  oggStream;
        vorbis_info*    vorbisInfo;
        vorbis_comment* vorbisComment;

        ALuint buffers[2];
        ALuint source;
        ALenum format;
};


#endif // NEZTU_OGG_H
