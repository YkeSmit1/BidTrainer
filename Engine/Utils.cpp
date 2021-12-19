#include "Utils.h"

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