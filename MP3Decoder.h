//+-----------------------------------------------------------------------------
//
//  Copyright (c) 2009 Matthew Loar.
//  See LICENSE for license terms.
//
//  Abstract:
//      Defines the MP3 decoder.
//
//------------------------------------------------------------------------------

#ifndef NEZTU_MP3DECODER_H
#define NEZTU_MP3DECODER_H

#include <mpg123.h>
#include "Player.h"

class MP3Decoder : public Decoder
{
    public:
        MP3Decoder(const char *filename);
        ~MP3Decoder();
        static bool CanHandle(const char *filename);
        virtual bool FillBuffer(ALuint buffer);

    private:
        mpg123_handle *mp3File;
        int encoding, channels, format;
        long rate;
};

#endif // NEZTU_MP3DECODER_H
