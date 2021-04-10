#pragma once

enum class Phase
{
    Unknown,
    Opening,
    OneSuit,
    OneNT,
    Stayman,
    JacobyHearts,
    JacobySpades
};

extern "C" __declspec(dllexport) int GetBidFromRule(Phase phase, const char* hand, int lastBidId, int position, 
    int minSpades, int minHearts, int minDiamonds, int minClubs, Phase* newPhase, char* description);
extern "C" __declspec(dllexport) void GetRulesByBid(Phase phase, int bidId, int position, char* information);
extern "C" __declspec(dllexport) int Setup(const char* database);