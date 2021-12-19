#include "Api.h"

#include <string>
#include <iostream>
#include <sstream>
#include "Rule.h"
#include "SQLiteCppWrapper.h"
#include <filesystem>
#include "Utils.h"
#include "BoardCharacteristic.h"

std::unique_ptr<ISQLiteWrapper> sqliteWrapper = nullptr;


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
    if (sqliteWrapper == nullptr)
        throw std::logic_error("Setup was not called to initialize sqlite database");
    return sqliteWrapper.get();
}

int GetBidFromRule(Phase phase, const char* hand, int lastBidId, int position, int* minSuitsPartner, int* minSuitsOpener, Phase* newPhase, char* description)
{
    auto handCharacteristic = GetHandCharacteristic(hand);
    auto minSuitsPartnerVec = std::vector<int>(minSuitsPartner, minSuitsPartner + 4);
    auto opponentsSuits = std::vector<int>(minSuitsOpener, minSuitsOpener + 4);
    auto boardCharacteristic = BoardCharacteristic::Create(handCharacteristic, minSuitsPartnerVec, opponentsSuits);

    auto [bidId, lNewfase, descr] = GetSqliteWrapper()->GetRule(handCharacteristic, boardCharacteristic, phase, lastBidId, position);
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
    sqliteWrapper = std::make_unique<SQLiteCppWrapper>(database);
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
    assert(linformation.size() < 8192);
    information[linformation.size()] = '\0';
}
