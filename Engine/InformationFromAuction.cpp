#include "pch.h"
#include "InformationFromAuction.h"
#include <algorithm>
#include "Utils.h"
#include "ISQLiteWrapper.h"
#include "nlohmann/json.hpp"

using namespace std::literals::string_literals;

InformationFromAuction::InformationFromAuction(ISQLiteWrapper* sqliteWrapper, const std::string& previousBidding)
{
    std::vector<std::vector<int>> minSuitLengths{ std::vector<int>{0, 0, 0, 0}, std::vector<int>{0, 0, 0, 0}, std::vector<int>{0, 0, 0, 0}, std::vector<int>{0, 0, 0, 0} };

    auto bidIds = Utils::SplitAuction(previousBidding);
    auto phaseOpener = Phase::Opening;
    auto phaseDefensive = Phase::Opening;
    auto position = 1;
    auto currentBidding = ""s;
    auto lastIdNonSlam = 13;
    auto partner = ((size_t)bidIds.size() + 2) % 4;

    for (auto& bidId : bidIds)
    {
        if (bidId != 0)
        {
            auto* currentPhase = position % 2 == 0 ? &phaseDefensive : &phaseOpener;
            auto player = ((size_t)position - 1) % 4;
            if (!isSlamBidding)
            {
                auto isCompetitive = Utils::GetIsCompetitive(currentBidding);
                auto rules = sqliteWrapper->GetInternalRulesByBid(*currentPhase, bidId, position, currentBidding, isCompetitive);
                if (rules.size() > 0)
                {
                    if (rules.at(0)["NextPhase"] != "")
                        *currentPhase = (Phase)std::stoi(rules.at(0)["NextPhase"]);
                    for (auto i = 0; i < 4; i++)
                        minSuitLengths.at(player).at(i) = std::max(minSuitLengths.at(player).at(i), GetLowestValue(rules, "Min"s + Utils::GetSuit(i)));
                    if (player == partner)
                    {
                        minHcpPartner = std::max(minHcpPartner, GetLowestValue(rules, "MinHcp"));
                    }
                }
            }
            else
            {
                auto rules = sqliteWrapper->GetInternalRelativeRulesByBid(bidId, currentBidding);
                if (rules.size() > 0)
                {
                    if (player == partner)
                    {
                        for (auto i = 0; i < 4; i++)
                            controls.at(i) = controls.at(i) || AllTrue(rules, Utils::GetSuit2(i) + "Control"s);
                        keyCardsPartner = std::max(keyCardsPartner, GetLowestValue(rules, "KeyCards"));
                        trumpQueenPartner = trumpQueenPartner || AllTrue(rules, "TrumpQueen");
                    }
                }
            }
            if (bidId == lastIdNonSlam)
            {
                isSlamBidding = true;
                currentBidding = "";
                position++;
                *currentPhase = Phase::SlamBidding;
                continue;
            }
        }
        if (!isSlamBidding || position % 2 != 0)
            currentBidding += Utils::GetBidASCII(bidId);
        position++;
    }

    phase = position % 2 == 0 ? phaseDefensive : phaseOpener;
    if (isSlamBidding)
        previousSlamBidding = currentBidding;

    partnersSuits = minSuitLengths.at(partner);
    openersSuits = minSuitLengths.at(0);
}

int InformationFromAuction::GetLowestValue(const std::vector<std::unordered_map<std::string, std::string>>& rules, std::string columnName)
{
    if (rules.size() == 0)
        return 0;
    auto minElement = std::min_element(rules.begin(), rules.end(), [&](const auto& a, const auto& b) {return std::stoi(a.at(columnName)) < std::stoi(b.at(columnName)); });
    auto &value = (*minElement).at(columnName);
    return value == "" ? 0 : std::stoi(value);
}

bool InformationFromAuction::AllTrue(const std::vector<std::unordered_map<std::string, std::string>>& rules, std::string columnName)
{
    if (rules.size() == 0)
        return false;
    auto s = std::all_of(rules.begin(), rules.end(), [&](const auto& a) {return a.at(columnName) != "" && std::stoi(a.at(columnName)) == 1; });
    return s;
}

std::string InformationFromAuction::AsJson()
{
    nlohmann::json j
    {
        {"minSuitLengthsPartner", partnersSuits},
        {"minHcpPartner", minHcpPartner},
        {"controls", controls},
        {"keyCardsPartner", keyCardsPartner},
        {"trumpQueenPartner", trumpQueenPartner}
    };

    std::stringstream ss;
    ss << j;
    return ss.str();
}
