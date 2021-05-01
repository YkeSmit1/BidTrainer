#pragma once

enum class Phase
{
    Unknown,
    Opening,
    OneSuit,
    OneNT,
    Stayman,
    JacobyHearts,
    JacobySpades,
    OverCall,
    TakeOutDbl,
    OneNTOvercall
};

enum class Suit
{
    Clubs = 0,
    Diamonds = 1,
    Hearts = 2,
    Spades = 3,
    NoTrump = 4
};


extern "C" __declspec(dllexport) int GetBidFromRule(Phase phase, const char* hand, int lastBidId, int position, 
    int* minSuitsPartner, int* minSuitsOpener, Phase* newPhase, char* description);
extern "C" __declspec(dllexport) void GetRulesByBid(Phase phase, int bidId, int position, char* information);
extern "C" __declspec(dllexport) int Setup(const char* database);