//+-----------------------------------------------------------------------------
//
//  Copyright (c) 2009 Matthew Loar.
//  See LICENSE for license terms.
//
//  Abstract:
//      Implements the MP3 decoder.
//
//------------------------------------------------------------------------------

#include <string>
#include "MP3Decoder.h"

MP3Decoder::MP3Decoder(const char *filename) :
    mp3File(filename)
{
    if (mpg123_getformat(mp3File, &rate, &channels, &encoding) != MPG123_OK)
    {
        throw std::string("Could not get format of MP3 file.");
    }

    encoding = MPG123_ENC_SIGNED_16;

    if (mpg123_format(mp3File, rate, channels, encoding) != MPG123_OK)
    {
        throw std::string("Could not set output format.");
    }

    if (mpg123_param(mp3File, MPG123_RVA, MPG123_RVA_MIX, 0) != MPG123_OK)
    {
        throw std::string("Could not enable replay gain.");
    }

    if(channels == MPG123_MONO)
        format = AL_FORMAT_MONO16;
    else if (channels == MPG123_STEREO)
        format = AL_FORMAT_STEREO16;
    else
        throw std::string("MP3 file has too many channels.");
}

// Class is polymorphic - virtual destructor required.
MP3Decoder::~MP3Decoder()
{
}

bool MP3Decoder::FillBuffer(ALuint buffer)
{
    const size_t buffer_size = 65536;
    unsigned char buf[buffer_size];
    size_t done = 0;

    int err = mpg123_read(mp3File, buf, buffer_size, &done);
    if (err != MPG123_OK && err != MPG123_DONE)
    {
        throw std::string("mpg123_read failed");
    }

    alBufferData(buffer, format, buf, done, rate);

    return err == MPG123_OK;
}

bool MP3Decoder::CanHandle(const char *filename)
{
    bool ret = false;
    mpg123_handle *mp3File;
    if (mpg123_init() == MPG123_OK)
    {
        int err;
        if ((mp3File = mpg123_new(NULL, &err)) != NULL)
        {
            if (mpg123_open(mp3File, const_cast<char*>(filename)) == MPG123_OK)
            {
                int channels, encoding;
                long rate;

                if (
                    mpg123_getformat(mp3File, &rate, &channels, &encoding)
                    == MPG123_OK
                    )
                {
                    if (
                        mpg123_format(mp3File, rate, channels, encoding)
                        == MPG123_OK
                        )
                    {
                        ret = true;
                    }
                }

                mpg123_close(mp3File);
            }
            mpg123_delete(mp3File);
        }
        mpg123_exit();
    }

    return ret;
}
