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
    OneNTOvercall,
    TwoNT,
    TwoClubs
};

enum class BidKind
{
    UnknownSuit,
    FirstSuit,
    SecondSuit,
    LowestSuit,
    HighestSuit,
    PartnersSuit,
    OpponentsSuit
};

enum class Modules
{
    FourCardMajors = 1,
    FiveCardMajors = 2,
    TwoBidsAndHigher = 4,
    NegativeDbl = 8,
    Reverse = 16,
    ControlBids = 32,
    RKC = 64
};

extern "C" {
    __declspec(dllexport) int GetBidFromRule(Phase phase, const char* hand, int lastBidId, int position,
        int* minSuitsPartner, int* minSuitsOpener, const char* previousBidding, bool isCompetitive, Phase* newPhase, char* description);
    __declspec(dllexport) void GetRulesByBid(Phase phase, int bidId, int position, const char* previousBidding, bool isCompetitive, char* information);
    __declspec(dllexport) int Setup(const char* database);
    __declspec(dllexport) void SetModules(int modules);
}