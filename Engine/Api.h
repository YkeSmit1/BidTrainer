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
    TwoClubs,
    SlamBidding
};

/// <summary>
/// Types of bids deduced from the hand
/// </summary>
enum class BidKind
{
    UnknownSuit,
    FirstSuit,
    SecondSuit,
    LowestSuit,
    HighestSuit,
    PartnersSuit,
};

/// <summary>
/// Type of bids deduced from the auction
/// </summary>
enum class BidKindAuction
{
    UnknownSuit,
    NonReverse,
    Reverse,
    OwnSuit,
    PartnersSuit,
    RespondingToDouble
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
    __declspec(dllexport) int GetBidFromRule(Phase phase, const char* hand, const char* previousBidding, Phase* newPhase, char* description);
    __declspec(dllexport) void GetRulesByBid(Phase phase, int bidId, int position, const char* previousBidding, bool isCompetitive, char* information);
    __declspec(dllexport) void GetRelativeRulesByBid(int bidId, const char* previousBidding, char* information);
        __declspec(dllexport) int Setup(const char* database);
    __declspec(dllexport) void SetModules(int modules);
    __declspec(dllexport) void GetInformationFromAuction(const char* previousBidding, char* informationFromAuctionjson);
}