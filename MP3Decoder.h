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

#include <stdexcept>
#include <mpg123.h>

#include "Player.h"

class MP3Decoder : public Decoder
{
public:
    MP3Decoder(const char *filename);
    virtual ~MP3Decoder();
    static bool CanHandle(const char *filename);
    virtual bool FillBuffer(ALuint buffer);

private:
    struct mpg123_initializer
    {
        mpg123_initializer()
        {
            if (mpg123_init() != MPG123_OK)
            {
                throw std::runtime_error(
                    "Could not initialize mpg123 library."
                    );
            }
        }

        ~mpg123_initializer()
        {
            mpg123_exit();
        }
    } m_mpg123_init;

    struct mpg123_handle_holder
    {
        mpg123_handle_holder(const char *filename)
        {
            int err;
            if ((m_handle = mpg123_new(NULL, &err)) == NULL)
            {
                throw std::runtime_error("mpg123_new() failed");
            }

            if (mpg123_open(m_handle, const_cast<char*>(filename)) != MPG123_OK)
            {
                mpg123_delete(m_handle);
                throw std::runtime_error("Could not open MP3 file.");
            }
        }

        ~mpg123_handle_holder()
        {
            mpg123_close(m_handle);
            mpg123_delete(m_handle);
        }

        operator mpg123_handle*()
        {
            return m_handle;
        }
    private:
        mpg123_handle *m_handle;
    } mp3File;

    int encoding, channels, format;
    long rate;
};

#endif // NEZTU_MP3DECODER_H
