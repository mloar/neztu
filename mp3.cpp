#include "mp3.h"

bool mp3_stream::try_open(const string &path)
{
    return mpeg3_check_sig((char*)path.c_str());
}

void mp3_stream::open(const char *path)
{
    if (!(mp3File = mpeg3_open(const_cast<char*>(path))))
        throw string("Could not open MP3 file.");

    mpeg3_set_mmx(mp3File, 0);
    if (!mpeg3_has_audio(mp3File))
    {
        throw string("MP3 file has no audio streams.");
    }

    if (mpeg3_total_astreams(mp3File) > 1)
    {
        throw string("MP3 file has multiple audio streams.");
    }

    if((channels = mpeg3_audio_channels(mp3File, 0)) == 1)
        format = AL_FORMAT_MONO16;
    else if (channels == 2)
        format = AL_FORMAT_STEREO16;
    else
        throw string("MP3 file has too many channels.");

    alGenBuffers(2, buffers);
    check();
    alGenSources(1, &source);
    check();
    
    alSource3f(source, AL_POSITION,        0.0, 0.0, 0.0);
    alSource3f(source, AL_VELOCITY,        0.0, 0.0, 0.0);
    alSource3f(source, AL_DIRECTION,       0.0, 0.0, 0.0);
    alSourcef (source, AL_ROLLOFF_FACTOR,  0.0          );
    alSourcei (source, AL_SOURCE_RELATIVE, AL_TRUE      );
}




mp3_stream::~mp3_stream()
{
    alSourceStop(source);
    empty();
    alDeleteSources(1, &source);
    check();
    alDeleteBuffers(2, buffers);
    check();
    mpeg3_close(mp3File);
}




void mp3_stream::display()
{
    cout
        << "channels        " << channels << "\n";
    /*cout
        << "version         " << vorbisInfo->version         << "\n"
        << "channels        " << vorbisInfo->channels        << "\n"
        << "rate (hz)       " << vorbisInfo->rate            << "\n"
        << "bitrate upper   " << vorbisInfo->bitrate_upper   << "\n"
        << "bitrate nominal " << vorbisInfo->bitrate_nominal << "\n"
        << "bitrate lower   " << vorbisInfo->bitrate_lower   << "\n"
        << "bitrate window  " << vorbisInfo->bitrate_window  << "\n"
        << "\n"
        << "vendor " << vorbisComment->vendor << "\n";
        
    for(int i = 0; i < vorbisComment->comments; i++)
        cout << "   " << vorbisComment->user_comments[i] << "\n";*/
        
    cout << endl;
}




bool mp3_stream::playback()
{
    if(playing())
        return true;
        
    if(!stream(buffers[0]))
        return false;
        
    if(!stream(buffers[1]))
        return false;
    
    alSourceQueueBuffers(source, 2, buffers);
    alSourcePlay(source);
    
    return true;
}




bool mp3_stream::playing()
{
    ALenum state;
    
    alGetSourcei(source, AL_SOURCE_STATE, &state);
    
    return (state == AL_PLAYING);
}




bool mp3_stream::update()
{
    int processed;
    bool active = true;

    alGetSourcei(source, AL_BUFFERS_PROCESSED, &processed);

    while(processed--)
    {
        ALuint buffer;
        
        alSourceUnqueueBuffers(source, 1, &buffer);
        check();

        active = stream(buffer);

        alSourceQueueBuffers(source, 1, &buffer);
        check();
    }

    return active;
}

bool mp3_stream::stream(ALuint buffer)
{
    short *audio_output_i = new short[BUFSIZE * channels];
    int result = 0;

    for(int i = 0; i < channels; i++)
    {
        if(i == 0)
            result = mpeg3_read_audio(mp3File, 
                    0, 
                    audio_output_i, 
                    i, 
                    BUFSIZE, 
                    0);
        else
            result = mpeg3_reread_audio(mp3File, 
                    0,      /* Pointer to pre-allocated buffer of floats */
                    audio_output_i + (i * BUFSIZE),      /* Pointer to pre-allocated buffer of int16's */
                    i,          /* Channel to decode */
                    BUFSIZE,         /* Number of samples to decode */
                    0);

        if (result)
        {
            throw string("mpeg3_read_audio failed");
        }

        if (channels > 1)
        {
            for (int j = 0; j < BUFSIZE; j++)
            {
                short temp = audio_output_i[j];
                audio_output_i[j] = audio_output_i[j + BUFSIZE];
                audio_output_i[j + BUFSIZE] = temp;
            }
        }
    }

    alBufferData(buffer, format, audio_output_i, BUFSIZE * channels, mpeg3_sample_rate(mp3File, 0) / 2);
    delete[] audio_output_i;
    check();

    return !mpeg3_end_of_audio(mp3File, 0);
}




void mp3_stream::empty()
{
    int queued;
    
    alGetSourcei(source, AL_BUFFERS_QUEUED, &queued);
    
    while(queued--)
    {
        ALuint buffer;
    
        alSourceUnqueueBuffers(source, 1, &buffer);
        check();
    }
}




void mp3_stream::check()
{
	int error = alGetError();

	if(error != AL_NO_ERROR)
		throw string("OpenAL error was raised.");
}



string mp3_stream::errorString(int code)
{
    switch(code)
    {
        default:
            return string("Unknown MP3 error.");
    }
}
