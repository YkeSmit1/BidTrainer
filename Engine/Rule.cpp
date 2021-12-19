#include "Rule.h"
#include "Utils.h"
#include <algorithm>
#include <cassert>
#include <iterator>

HandCharacteristic::HandCharacteristic(const std::string& hand)
{
    Initialize(hand);
}

void HandCharacteristic::Initialize(const std::string& hand)
{
    this->hand = hand;
    assert(hand.length() == 16);

    auto suits = Utils::Split<char>(hand, ',');
    assert(suits.size() == 4);
    suitLengths.clear();
    std::transform(suits.begin(), suits.end(), std::back_inserter(suitLengths), [](const auto& x) {return (int)x.length(); });
    auto suitLengthSorted = suitLengths;

    std::sort(suitLengthSorted.begin(), suitLengthSorted.end(), std::greater<>{});
    lengthFirstSuit = suitLengthSorted.at(0);
    lengthSecondSuit = suitLengthSorted.at(1);

    firstSuit = lengthFirstSuit == lengthSecondSuit ? -1 : (int)std::distance(suitLengths.begin(), std::find(suitLengths.begin(), suitLengths.end(), lengthFirstSuit));
    secondSuit = lengthFirstSuit == lengthSecondSuit ? -1 : (int)std::distance(suitLengths.begin(), std::find(suitLengths.begin(), suitLengths.end(), lengthSecondSuit));

    highestSuit = lengthFirstSuit != lengthSecondSuit ? -1 : (int)std::distance(suitLengths.begin(), std::find(suitLengths.begin(), suitLengths.end(), lengthFirstSuit));
    lowestSuit = lengthFirstSuit != lengthSecondSuit ? -1 : (int)std::distance(std::find(suitLengths.rbegin(), suitLengths.rend(), lengthFirstSuit), suitLengths.rend()) - 1;

    std::sort(suits.begin(), suits.end(), [](const auto& l, const auto& r) noexcept {return l.length() > r.length(); });
    auto distribution = std::to_string(suits[0].length()) + std::to_string(suits[1].length()) +
        std::to_string(suits[2].length())  + std::to_string(suits[3].length());
    isBalanced = distribution == "4333" || distribution == "4432" || distribution == "5332";
    Hcp = CalculateHcp(hand);
}

int HandCharacteristic::CalculateHcp(const std::string& hand)
{
    const auto aces = NumberOfCards(hand, 'A');
    const auto kings = NumberOfCards(hand, 'K');
    const auto queens = NumberOfCards(hand, 'Q');
    const auto jacks = NumberOfCards(hand, 'J');
    return aces * 4 + kings * 3 + queens * 2 + jacks;
}

int HandCharacteristic::NumberOfCards(const std::string& hand, char card)
{
    return (int)std::count_if(hand.begin(), hand.end(), [card](auto c) {return c == card; });
}
