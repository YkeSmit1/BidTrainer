#include "pch.h"
#include "Utils.h"
#include <algorithm>

std::string Utils::GetSuit(int bidId)
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
