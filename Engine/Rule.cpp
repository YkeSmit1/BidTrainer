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
    std::ranges::transform(suits, std::back_inserter(suitLengths), [](const auto& x) {return (int)x.length(); });
    auto suitLengthSorted = suitLengths;

    std::ranges::sort(suitLengthSorted, std::greater{});
    lengthFirstSuit = suitLengthSorted.at(0);
    lengthSecondSuit = suitLengthSorted.at(1);

    auto suitsEqualLength = lengthFirstSuit == lengthSecondSuit;

    firstSuit = suitsEqualLength ? -1 : (int)std::distance(suitLengths.begin(), std::ranges::find(suitLengths, lengthFirstSuit));
    secondSuit = suitsEqualLength ? -1 : (int)std::distance(suitLengths.begin(), std::ranges::find(suitLengths, lengthSecondSuit));

    highestSuit = !suitsEqualLength ? -1 : (int)std::distance(suitLengths.begin(), std::ranges::find(suitLengths, lengthFirstSuit));
    lowestSuit = !suitsEqualLength ? -1 : (int)std::distance(std::ranges::find(suitLengths.rbegin(), suitLengths.rend(), lengthFirstSuit), suitLengths.rend()) - 1;

    Hcp = Utils::CalculateHcp(hand);
    controls.clear();
    for (auto suit = 0; suit <= 3; suit++)
    {
        controls.push_back(GetHasControl(suits.at(suit)));
    }

    std::ranges::sort(suits, [](const auto& l, const auto& r) noexcept {return l.length() > r.length(); });
    auto distribution = std::to_string(suits[0].length()) + std::to_string(suits[1].length()) +
        std::to_string(suits[2].length())  + std::to_string(suits[3].length());
    isBalanced = distribution == "4333" || distribution == "4432" || distribution == "5332";
    isSemiBalanced = isBalanced || distribution == "5422" || distribution == "4441" || distribution == "5431";
    isReverse = firstSuit > secondSuit && !suitsEqualLength;
}

bool HandCharacteristic::GetHasControl(const std::string& suit)
{
    return suit.length() <= 1 || Utils::NumberOfCards(suit, 'A') == 1 || Utils::NumberOfCards(suit, 'K') == 1;
}
