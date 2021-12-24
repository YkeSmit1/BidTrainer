#include "pch.h"
#include "Utils.h"
#include <algorithm>

std::string Utils::GetSuitPretty(int bidId)
{
    auto suit = 4 - (bidId % 5);
    switch (suit)
    {
    case 0: return "Spades";
    case 1: return "Hearts";
    case 2: return "Diamonds";
    case 3: return "Clubs";
    default:
        throw new std::invalid_argument("Unknown suit");
    }
}

int Utils::GetSuitInt(int bidId)
{
    return 4 - (bidId % 5);
}

int Utils::GetSuit(const std::string& suit)
{
    if (suit == "NT")
        return -1;
    if (suit == "S")
        return 0;
    if (suit == "H")
        return 1;
    if (suit == "D")
        return 2;
    if (suit == "C")
        return 3;
    throw new std::invalid_argument("Unknown suit");
}

std::string Utils::GetSuitASCII(int bidId)
{
    auto suit = 4 - (bidId % 5);
    switch (suit)
    {
    case 0: return "S";
    case 1: return "H";
    case 2: return "D";
    case 3: return "C";
    default:
        throw new std::invalid_argument("Unknown suit");
    }
}

std::vector<int> Utils::SplitAuction(const std::string& auction)
{
    std::vector<int> ret{};
    std::string currentBid = "";
    for (auto& c : auction)
    {
        if (currentBid.length() > 0 && (isdigit(c) || c == 'P' || c == 'X'))
        {
            if (currentBid != "")
                ret.push_back(GetBidId(currentBid));
            currentBid = "";
        }
        currentBid += c;
    }
    if (currentBid != "")
        ret.push_back(GetBidId(currentBid));

    return ret;
}

int Utils::GetBidId(const std::string& bid)
{
    if (bid == "Pass")
        return 0;
    if (bid == "X")
        return -1;
    if (bid == "XX")
        return -2;
    auto rank = std::stoi(bid.substr(0, 1));
    auto suit = GetSuit(bid.substr(1, bid.length() - 1));
    return (rank - 1) * 5 + 4 - suit;
}


int Utils::GetRank(int bidId)
{
    return (int)((bidId - 1) / 5) + 1;
}

int Utils::NumberOfCards(const std::string& hand, char card)
{
    return (int)std::count_if(hand.begin(), hand.end(), [card](auto c) {return c == card; });
}

int Utils::CalculateHcp(const std::string& hand)
{
    const auto aces = Utils::NumberOfCards(hand, 'A');
    const auto kings = Utils::NumberOfCards(hand, 'K');
    const auto queens = Utils::NumberOfCards(hand, 'Q');
    const auto jacks = Utils::NumberOfCards(hand, 'J');
    return aces * 4 + kings * 3 + queens * 2 + jacks;
}
