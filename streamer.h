#ifndef NEZTU_STREAMER_H
#define NEZTU_STREAMER_H

#include <string>

class streamer
{
    public:
    virtual void open(const char*) = 0;
    virtual void display() = 0;
    virtual bool playback() = 0;
    virtual bool playing() = 0;
    virtual bool update() = 0;
};

#endif // NEZTU_STREAMER_H
