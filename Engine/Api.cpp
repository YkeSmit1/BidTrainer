#include "pch.h"
#include "Api.h"

#include <string>
#include <iostream>
#include <sstream>
#include "Rule.h"
#include <unordered_map>
#include "SQLiteCppWrapper.h"
#include <filesystem>

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

int GetBidFromRule(Phase phase, const char* hand, int lastBidId, int position, int minSpades, int minHearts, int minDiamonds, int minClubs, Phase* newPhase, char* description)
{
    auto handCharacteristic = GetHandCharacteristic(hand);
    auto fitSpades = handCharacteristic.Spades + minSpades >= 8;
    auto fitHearts = handCharacteristic.Hearts + minHearts >= 8;
    auto fitDiamonds = handCharacteristic.Diamonds + minDiamonds >= 8;
    auto fitClubs = handCharacteristic.Clubs + minClubs >= 8;

    std::vector<bool> fits = { fitSpades, fitHearts, fitDiamonds, fitClubs};

    auto [bidId, lNewfase, descr] = GetSqliteWrapper()->GetRule(handCharacteristic, fits, phase, lastBidId, position);
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
