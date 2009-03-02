//+-----------------------------------------------------------------------------
//
//  Copyright (c) 2009 Matthew Loar.
//  See LICENSE for license terms.
//
//  Abstract:
//      Implements the Player class.
//
//------------------------------------------------------------------------------

#include <iostream>
#include <iomanip>
#include <string>

#include <stdlib.h>
#include <AL/al.h>
#include <AL/alut.h>

#include "Player.h"
#include "MP3Decoder.h"
#include "OggVorbisDecoder.h"

#define BUFCOUNT 4

Player::Player(const char *filename, bool (*cancel_func)()) :
    m_cancel_func(cancel_func),
    m_decoder(NULL)
{
    if (OggVorbisDecoder::CanHandle(filename))
    {
        m_decoder = new OggVorbisDecoder(filename);
    }
    else if (MP3Decoder::CanHandle(filename))
    {
        m_decoder = new MP3Decoder(filename);
    }
    else
    {
        throw std::string("unsupported file type");
    }

}

Player::~Player()
{
    if (m_decoder)
        delete m_decoder;
}


void Player::Play()
{
    ALuint source;
    ALuint buffers[BUFCOUNT];

    alutInit(NULL, NULL);
    alGenBuffers(BUFCOUNT, buffers);
    check();
    alGenSources(1, &source);
    check();

    alSource3f(source, AL_POSITION,        0.0, 0.0, 0.0);
    check();
    alSource3f(source, AL_VELOCITY,        0.0, 0.0, 0.0);
    check();
    alSource3f(source, AL_DIRECTION,       0.0, 0.0, 0.0);
    check();
    alSourcef (source, AL_ROLLOFF_FACTOR,  0.0          );
    check();
    alSourcei (source, AL_SOURCE_RELATIVE, AL_TRUE      );
    check();

    for (int i = 0; i < BUFCOUNT; i++)
    {
        if(!m_decoder->FillBuffer(buffers[i]))
            goto cleanup;
        check();
    }

    alSourceQueueBuffers(source, BUFCOUNT, buffers);
    check();

    alSourcePlay(source);
    check();

    {
        bool active = true;

        do
        {
            int processed;
            alGetSourcei(source, AL_BUFFERS_PROCESSED, &processed);
            check();

            while (processed--)
            {
                ALuint buffer;

                alSourceUnqueueBuffers(source, 1, &buffer);
                check();

                active = m_decoder->FillBuffer(buffer);
                check();

                alSourceQueueBuffers(source, 1, &buffer);
                check();

                ALenum state;
                alGetSourcei(source, AL_SOURCE_STATE, &state);
                check();
                if (state != AL_PLAYING)
                {
                    alSourcePlay(source);
                }
            }

            usleep(1000);
        } while (active && !m_cancel_func());
    }

cleanup:
    alSourceStop(source);
    check();
    alSourcei(source, AL_BUFFER, AL_NONE);
    check();
    alDeleteSources(1, &source);
    check();
    alDeleteBuffers(BUFCOUNT, buffers);
    check();
    alutExit();
}

void Player::check()
{
    int error = alGetError();

    if(error != AL_NO_ERROR)
    {
        std::cerr << error << std::endl;
        abort();
        throw std::string("OpenAL error was raised.");
    }
}
