//+-----------------------------------------------------------------------------
//
//  Copyright (c) 2009 Matthew Loar.
//  See LICENSE for license terms.
//
//  Abstract:
//      Defines the Player interface.
//
//------------------------------------------------------------------------------

#ifndef NEZTU_PLAYER_H
#define NEZTU_PLAYER_H

#include <AL/al.h>

class Decoder
{
    public:
        virtual bool FillBuffer(ALuint buffer) = 0;
};

class Player
{
    public:
        Player(const char *filename, bool (*cancel_func)());
        ~Player();
        void Play();

    private:
        bool (*m_cancel_func)();
        Decoder *m_decoder;

        void check();
};

#endif // NEZTU_PLAYER_H
