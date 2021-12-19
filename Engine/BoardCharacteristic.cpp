#include "BoardCharacteristic.h"
#include <vector>
#include <algorithm>
#include "Rule.h"
#include "Utils.h"

BoardCharacteristic BoardCharacteristic::Create(HandCharacteristic hand, const std::vector<int>& partnersSuits, const std::vector<int>& opponentsSuits)
{
    BoardCharacteristic boardCharacteristic{};

    std::vector<bool> fits;
    boardCharacteristic.partnersSuit = GetSuit(partnersSuits);
    std::transform(hand.suitLengths.begin(), hand.suitLengths.end(), partnersSuits.begin(), std::back_inserter(fits),
        [](const auto& x, const auto& y) {return x + y >= 8; });

    boardCharacteristic.opponentsSuit = GetSuit(opponentsSuits);
    boardCharacteristic.stopInOpponentsSuit = GetHasStopInOpponentsSuit(hand.hand, boardCharacteristic.opponentsSuit);
    boardCharacteristic.hasFit = std::find(fits.begin(), fits.end(), true) != fits.end();
    boardCharacteristic.fitIsMajor = fits.at(0) || fits.at(1);
    return boardCharacteristic;

}

int BoardCharacteristic::GetSuit(const std::vector<int>& suitLengths)
{
    return !std::any_of(suitLengths.begin(), suitLengths.end(), [](const auto& x) {return x > 0; }) ? -1 :
        (int)std::distance(suitLengths.begin(), std::max_element(suitLengths.begin(), suitLengths.end()));
}

bool BoardCharacteristic::GetHasStopInOpponentsSuit(const std::string& hand, int opponentsSuit)
{
    if (opponentsSuit == -1)
        return false;
    auto cardsInOpponentSuit = Utils::Split<char>(hand, ',')[opponentsSuit];
    if (cardsInOpponentSuit.length() == 0)
        return false;

    switch (cardsInOpponentSuit[0])
    {
    case 'A': return true;
    case 'K': return cardsInOpponentSuit.length() >= 2;
    case 'Q': return cardsInOpponentSuit.length() >= 3;
    case 'J': return cardsInOpponentSuit.length() >= 4;
    default:
        return false;
    }
}