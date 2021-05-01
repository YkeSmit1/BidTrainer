#include "pch.h"
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
    std::sort(suits.begin(), suits.end(), [] (const auto& l, const auto& r) noexcept {return l.length() > r.length();});
    distribution = std::to_string(suits[0].length()) + std::to_string(suits[1].length()) + 
        std::to_string(suits[2].length())  + std::to_string(suits[3].length());
    isBalanced = distribution == "4333" || distribution == "4432" || distribution == "5332";
    isThreeSuiter = CalcuateIsThreeSuiter(suitLengths);
    isReverse = !isBalanced && !isThreeSuiter && CalcuateIsReverse(suitLengths);
    Hcp = CalculateHcp(hand);
}

bool HandCharacteristic::CalcuateIsReverse(const std::vector<int>& suitLengths)
{
    std::vector<int> longSuits;
    std::copy_if(suitLengths.begin(), suitLengths.end(), std::inserter(longSuits, longSuits.begin()), [] (const auto &x) {return x > 3;});
    return longSuits.size() > 1 && longSuits.at(1) == 4;
}

bool HandCharacteristic::CalcuateIsThreeSuiter(const std::vector<int>& suitLength)
{
    return std::count_if(suitLength.begin(), suitLength.end(), [] (const auto &x) {return x > 3;}) == 3;
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
    return (int)std::count_if(hand.begin(), hand.end(), [card](char c) {return c == card; });
}
