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
    std::unordered_map<int, size_t> suitLength = {
        {0, suits[0].length()}, 
        {1, suits[1].length()},
        {2, suits[2].length()}, 
        {3, suits[3].length()}};

    Spades = (int)suitLength.at(0);
    Hearts = (int)suitLength.at(1);
    Diamonds = (int)suitLength.at(2);
    Clubs = (int)suitLength.at(3);

    std::sort(suits.begin(), suits.end(), [] (const auto& l, const auto& r) noexcept {return l.length() > r.length();});
    distribution = std::to_string(suits[0].length()) + std::to_string(suits[1].length()) + 
        std::to_string(suits[2].length())  + std::to_string(suits[3].length());
    isBalanced = distribution == "4333" || distribution == "4432" || distribution == "5332";
    isThreeSuiter = CalcuateIsThreeSuiter(suitLength);
    isReverse = !isBalanced && !isThreeSuiter && CalcuateIsReverse(suitLength);
    Hcp = CalculateHcp(hand);
}

bool HandCharacteristic::CalcuateIsReverse(const std::unordered_map<int, size_t>& suitLength)
{
    std::unordered_map<int, size_t> longSuits;
    std::copy_if(suitLength.begin(), suitLength.end(), std::inserter(longSuits, longSuits.begin()), [] (const auto &pair) {return pair.second > 3;});
    return longSuits.begin()->second == 4;
}

bool HandCharacteristic::CalcuateIsThreeSuiter(const std::unordered_map<int, size_t>& suitLength)
{
    return std::count_if(suitLength.begin(), suitLength.end(), [] (const auto &pair) {return pair.second > 3;}) == 3;
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
