#include "pch.h"
#include "Api.h"

#include <string>
#include <iostream>
#include <sstream>
#include "Rule.h"
#include "SQLiteCppWrapper.h"
#include <filesystem>
#include "Utils.h"

HandCharacteristic GetHandCharacteristic(const std::string& hand)
{
    static HandCharacteristic handCharacteristic{};
    if (hand != handCharacteristic.hand)
    {
        handCharacteristic.Initialize(hand);
    }
    return handCharacteristic;
}

ISQLiteWrapper* GetSqliteWrapper()
{
    static std::unique_ptr<ISQLiteWrapper> sqliteWrapper = std::make_unique<SQLiteCppWrapper>("four_card_majors.db3");
    return sqliteWrapper.get();
}

bool GetHasStopInOponentsSuit(std::string hand, Suit oponentsSuit)
{
    auto suit = 3 - int(oponentsSuit);
    auto cardsInOponentSuit = Utils::Split<char>(hand, ',')[suit];
    if (cardsInOponentSuit.length() == 0)
        return false;

    switch (cardsInOponentSuit[0])
    {
    case 'A': return true;
    case 'K': return cardsInOponentSuit.length() >= 2;
    case 'Q': return cardsInOponentSuit.length() >= 3;
    case 'J': return cardsInOponentSuit.length() >= 4;
    default:
        return false;
    }
}

int GetBidFromRule(Phase phase, const char* hand, int lastBidId, int position, int* minSuitsPartner, int* minSuitsOpener, Phase* newPhase, char* description)
{
    auto handCharacteristic = GetHandCharacteristic(hand);
    auto minSuitsPartnerVec = std::vector<int>(minSuitsPartner, minSuitsPartner + 4);
    auto fitSpades = handCharacteristic.suitLengths[0] + minSuitsPartner[(int)Suit::Spades] >= 8;
    auto fitHearts = handCharacteristic.suitLengths[1] + minSuitsPartner[(int)Suit::Hearts] >= 8;
    auto fitDiamonds = handCharacteristic.suitLengths[2] + minSuitsPartner[(int)Suit::Diamonds] >= 8;
    auto fitClubs = handCharacteristic.suitLengths[3] + minSuitsPartner[(int)Suit::Clubs] >= 8;
    auto fits = std::vector<bool>({ fitClubs, fitDiamonds, fitHearts, fitSpades });

    auto oponentsSuits = std::vector<int>(minSuitsOpener, minSuitsOpener + 4);
    auto oponentsSuit = (Suit)std::distance(oponentsSuits.begin(), std::max_element(oponentsSuits.begin(), oponentsSuits.end()));
    auto stopInOponentsSuit = GetHasStopInOponentsSuit(hand, oponentsSuit);

    auto [bidId, lNewfase, descr] = GetSqliteWrapper()->GetRule(handCharacteristic, fits, oponentsSuit, stopInOponentsSuit, phase, lastBidId, position);
    strncpy(description, descr.c_str(), descr.size());
    description[descr.size()] = '\0';
    *newPhase = lNewfase;
    return bidId;
}

int Setup(const char* database)
{
    using std::filesystem::path;

    if (!exists(path(database)))
        return -1;
    GetSqliteWrapper()->SetDatabase(database);
    return 0;
}

void GetBid(int bidId, int& rank, int& suit)
{
    GetSqliteWrapper()->GetBid(bidId, rank, suit);
}

void GetRulesByBid(Phase phase, int bidId, int position, char* information)
{
    auto linformation = GetSqliteWrapper()->GetRulesByBid(phase, bidId, position);
    strncpy(information, linformation.c_str(), linformation.size());
    information[linformation.size()] = '\0';
}
