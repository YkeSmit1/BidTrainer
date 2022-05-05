#include "Api.h"

#include <string>
#include <filesystem>
#include "Rule.h"
#include "SQLiteCppWrapper.h"
#include "BoardCharacteristic.h"
#include "InformationFromAuction.h"

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

int GetBidFromRule(const char* hand, const char* previousBidding, char* description)
{    
    auto handCharacteristic = GetHandCharacteristic(hand);
    InformationFromAuction informationFromAuction{ GetSqliteWrapper(), previousBidding};
    BoardCharacteristic boardCharacteristic{ handCharacteristic, previousBidding, informationFromAuction };

    auto isSlambidding = informationFromAuction.isSlamBidding || ((handCharacteristic.Hcp + boardCharacteristic.minHcpPartner >= 29 && boardCharacteristic.hasFit));

    auto [bidId, descr] = !isSlambidding ?
        GetSqliteWrapper()->GetRule(handCharacteristic, boardCharacteristic, previousBidding) :
        GetSqliteWrapper()->GetRelativeRule(handCharacteristic, boardCharacteristic, informationFromAuction.previousSlamBidding);
    assert(descr.size() < 128);
    strcpy(description, descr.c_str());
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

void GetRulesByBid(int bidId, const char* previousBidding, char* information)
{
    InformationFromAuction informationFromAuction{ GetSqliteWrapper(), previousBidding };
    std::string linformation;
    if (informationFromAuction.isSlamBidding)
        lInformation = GetSqliteWrapper()->GetRelativeRulesByBid(bidId, informationFromAuction.previousSlamBidding);
    else
        lInformation = GetSqliteWrapper()->GetRulesByBid(bidId, previousBidding);
    assert(lInformation.size() < 8192);
    strcpy(information, lInformation.c_str());
}

void SetModules(int modules)
{
    GetSqliteWrapper()->SetModules(modules);
}

void GetInformationFromAuction(const char* previousBidding, char* informationFromAuctionJson)
{
    InformationFromAuction informationFromAuction{ GetSqliteWrapper(), previousBidding };
    auto json = informationFromAuction.AsJson();
    assert(json.size() < 8192);
    strcpy(informationFromAuctionJson, json.c_str());
}
