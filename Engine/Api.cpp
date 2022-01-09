#include "pch.h"
#include "Api.h"

#include <string>
#include <iostream>
#include <sstream>
#include <filesystem>
#include <algorithm>
#include "Rule.h"
#include "SQLiteCppWrapper.h"
#include "Utils.h"
#include "BoardCharacteristic.h"
#include "InformationFromAuction.h"

using namespace std::literals::string_literals;

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

int GetLowestValue(const std::vector<std::unordered_map<std::string, std::string>>& rules, std::string columnName)
{
    if (rules.size() == 0)
        return 0;
   auto minElement = std::min_element(rules.begin(), rules.end(), [&](const auto& a, const auto& b) {return std::stoi(a.at(columnName)) < std::stoi(b.at(columnName)); });
   return std::stoi((*minElement).at(columnName));
}

bool AllTrue(const std::vector<std::unordered_map<std::string, std::string>>& rules, std::string columnName)
{
    if (rules.size() == 0)
        return false;
    auto s = std::all_of(rules.begin(), rules.end(), [&](const auto& a) {return a.at(columnName) != "" && std::stoi(a.at(columnName)) == 1; });
    return s;
}

InformationFromAuction GetInformationFromAuction(const std::string& previousBidding)
{
    InformationFromAuction informationFromAuction{};
    auto bidIds = Utils::SplitAuction(previousBidding);
    auto phaseOpener = Phase::Opening;
    auto phaseDefensive = Phase::Opening;
    auto position = 1;
    auto currentBidding = ""s;
    auto lastIdNonSlam = 13;

    for (auto& bidId : bidIds)
    {
        if (bidId != 0)
        {
            auto* currentPhase = position % 2 == 0 ? &phaseDefensive : &phaseOpener;
            if (!informationFromAuction.isSlamBidding)
            {
                auto isCompetitive = Utils::GetIsCompetitive(currentBidding);
                auto rules = GetSqliteWrapper()->GetInternalRulesByBid(*currentPhase, bidId, position, currentBidding, isCompetitive);
                if (rules.size() > 0)
                {
                    if (rules.at(0)["NextPhase"] != "")
                        *currentPhase = (Phase)std::stoi(rules.at(0)["NextPhase"]);
                    auto player = ((size_t)position - 1) % 4;
                    for (auto i = 0; i < 4; i++)
                        informationFromAuction.minSuitLengths.at(player).at(i) = max(informationFromAuction.minSuitLengths.at(player).at(i), GetLowestValue(rules, "Min"s + Utils::GetSuit(i)));
                    informationFromAuction.minHcps.at(player) = max(informationFromAuction.minHcps.at(player), GetLowestValue(rules, "MinHcp"));
                }
            }
            else
            {
                auto rules = GetSqliteWrapper()->GetInternalRelativeRulesByBid(bidId, currentBidding);
                if (rules.size() > 0)
                {
                    for (auto i = 0; i < 4; i++)
                        informationFromAuction.controls.at(i) = informationFromAuction.controls.at(i) || AllTrue(rules, Utils::GetSuit2(i) + "Control"s);
                }
            }
            if (bidId == lastIdNonSlam)
            {
                informationFromAuction.isSlamBidding = true;
                currentBidding = "";
                position++;
                *currentPhase = Phase::SlamBidding;
                continue;
            }
        }
        if (!informationFromAuction.isSlamBidding || position % 2 != 0)
            currentBidding += Utils::GetBidASCII(bidId);
        position++;
    }

    informationFromAuction.phase = position % 2 == 0 ? phaseDefensive : phaseOpener;
    if (informationFromAuction.isSlamBidding)
        informationFromAuction.previousSlamBidding = currentBidding;

    return informationFromAuction;
}

int GetBidFromRule(Phase phase, const char* hand, const char* previousBidding, Phase* newPhase, char* description)
{    
    auto handCharacteristic = GetHandCharacteristic(hand);
    auto informationFromAuction = GetInformationFromAuction(previousBidding);
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