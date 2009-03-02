//+-----------------------------------------------------------------------------
//
//  Copyright (c) 2009 Matthew Loar.
//  See LICENSE for license terms.
//
//  Abstract:
//      Defines the Ogg Vorbis decoder.
//
//------------------------------------------------------------------------------

#ifndef NEZTU_OGGVORBISDECODER_H
#define NEZTU_OGGVORBISDECODER_H

#include <string>

#include <ogg/ogg.h>
#include <vorbis/vorbisfile.h>

#include "Player.h"

class OggVorbisDecoder: public Decoder
{
    public:
        OggVorbisDecoder(const char *filename);
        ~OggVorbisDecoder();
        static bool CanHandle(const char *filename);
        virtual bool FillBuffer(ALuint buffer);

    protected:
        std::string errorString(int code);

    private:
        int             format;
        FILE*           oggFile;
        OggVorbis_File  oggStream;
        vorbis_info*    vorbisInfo;
        vorbis_comment* vorbisComment;
};

#endif // NEZTU_OGGVORBISDECODER_H
