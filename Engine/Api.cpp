#include "pch.h"
#include "Api.h"

#include <string>
#include <filesystem>
#include "Rule.h"
#include "SQLiteCppWrapper.h"
#include "BoardCharacteristic.h"
#include "InformationFromAuction.h"

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

int GetBidFromRule(Phase phase, const char* hand, const char* previousBidding, Phase* newPhase, char* description)
{    
    auto handCharacteristic = GetHandCharacteristic(hand);
    InformationFromAuction informationFromAuction{ GetSqliteWrapper(), previousBidding};
    BoardCharacteristic boardCharacteristic{ handCharacteristic, previousBidding, informationFromAuction };

    auto [bidId, lNewfase, descr] = informationFromAuction.phase != Phase::SlamBidding ?
        GetSqliteWrapper()->GetRule(handCharacteristic, boardCharacteristic, informationFromAuction.phase, previousBidding) :
        GetSqliteWrapper()->GetRelativeRule(handCharacteristic, boardCharacteristic, informationFromAuction.previousSlamBidding);
    assert(descr.size() < 128);
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

void GetRulesByBid(Phase phase, int bidId, int position, const char* previousBidding, bool isCompetitive, char* information)
{
    auto linformation = GetSqliteWrapper()->GetRulesByBid(phase, bidId, position, previousBidding, isCompetitive);
    assert(linformation.size() < 8192);
    strncpy(information, linformation.c_str(), linformation.size());
    information[linformation.size()] = '\0';
}

void GetRelativeRulesByBid(int bidId, const char* previousBidding, char* information)
{
    auto linformation = GetSqliteWrapper()->GetRelativeRulesByBid(bidId, previousBidding);
    assert(linformation.size() < 8192);
    strncpy(information, linformation.c_str(), linformation.size());
    information[linformation.size()] = '\0';
}


void SetModules(int modules)
{
    GetSqliteWrapper()->SetModules(modules);
}

void GetInformationFromAuction(const char* previousBidding, char* informationFromAuctionjson)
{
    InformationFromAuction informationFromAuction{ GetSqliteWrapper(), previousBidding };
    auto json = informationFromAuction.AsJson();
    assert(json.size() < 8192);
    strncpy(informationFromAuctionjson, json.c_str(), json.size());
    informationFromAuctionjson[json.size()] = '\0';
}
