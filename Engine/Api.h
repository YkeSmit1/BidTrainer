#pragma once

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
    __declspec(dllexport) int GetBidFromRule(const char* hand, const char* previousBidding, char* description);
    __declspec(dllexport) void GetRulesByBid(int bidId, const char* previousBidding, char* information);
    __declspec(dllexport) int Setup(const char* database);
    __declspec(dllexport) void SetModules(int modules);
    __declspec(dllexport) void GetInformationFromAuction(const char* previousBidding, char* informationFromAuctionJson);
}