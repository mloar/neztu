//+-----------------------------------------------------------------------------
//
//  Copyright (c) 2009 Matthew Loar.
//  See LICENSE for license terms.
//
//  Abstract:
//      Implements the OggVorbis decoder.
//
//------------------------------------------------------------------------------

#include <iostream>
#include <string>

#include "OggVorbisDecoder.h"

#define BUFFER_SIZE 65536

OggVorbisDecoder::OggVorbisDecoder(const char *path)
{
    int result;
    FILE *oggFile;

    if(!(oggFile = fopen(path, "rb")))
        throw std::string("Could not open Ogg file.");

    if((result = ov_open(oggFile, &oggStream, NULL, 0)) < 0)
    {
        fclose(oggFile);
        throw std::string("Could not open Ogg stream. ") + errorString(result);
    }

    vorbisInfo = ov_info(&oggStream, -1);
    vorbisComment = ov_comment(&oggStream, -1);

    if(vorbisInfo->channels == 1)
        format = AL_FORMAT_MONO16;
    else
        format = AL_FORMAT_STEREO16;
}

// Class is polymorphic - virtual destructor required.
OggVorbisDecoder::~OggVorbisDecoder()
{
    ov_clear(&oggStream);
}

bool OggVorbisDecoder::FillBuffer(ALuint buffer)
{
    char pcm[BUFFER_SIZE];
    int  size = 0;
    int  section;
    int  result;

    while(size < BUFFER_SIZE)
    {
        result = ov_read(&oggStream, pcm + size, BUFFER_SIZE - size, 0, 2, 1, &section);

        if(result > 0)
            size += result;
        else
            if(result < 0)
                throw errorString(result);
            else
                break;
    }

    if(size == 0)
        return false;

    alBufferData(buffer, format, pcm, size, vorbisInfo->rate);

    return true;
}

std::string OggVorbisDecoder::errorString(int code)
{
    switch(code)
    {
        case OV_EREAD:
            return std::string("Read from media.");
        case OV_ENOTVORBIS:
            return std::string("Not Vorbis data.");
        case OV_EVERSION:
            return std::string("Vorbis version mismatch.");
        case OV_EBADHEADER:
            return std::string("Invalid Vorbis header.");
        case OV_EFAULT:
            return std::string("Internal logic fault (bug or heap/stack corruption.");
        default:
            return std::string("Unknown Ogg error.");
    }
}

bool OggVorbisDecoder::CanHandle(const char *path)
{
    FILE *oggFile;
    OggVorbis_File oggStream;
    bool ret = false;

    if((oggFile = fopen(path, "rb")))
    {
        ret = !ov_test(oggFile, &oggStream, NULL, 0);
        ov_clear(&oggStream);
    }

    return ret;
}

