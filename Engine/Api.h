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

extern "C" {
    __declspec(dllexport) int GetBidFromRule(Phase phase, const char* hand, int lastBidId, int position,
        int* minSuitsPartner, int* minSuitsOpener, Phase* newPhase, char* description);
    __declspec(dllexport) void GetRulesByBid(Phase phase, int bidId, int position, char* information);
    __declspec(dllexport) int Setup(const char* database);
}