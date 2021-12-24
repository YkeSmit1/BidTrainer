#include "pch.h"
#include <iostream>
#include <algorithm>
#include <vector>
#include <unordered_map>

#include "SQLiteCppWrapper.h"
#include "nlohmann/json.hpp"

#include "Rule.h"
#include "Utils.h"
#include "Api.h"
#include "BoardCharacteristic.h"

SQLiteCppWrapper::SQLiteCppWrapper(const std::string& database)
{
    SetDatabase(database);
}

void SQLiteCppWrapper::SetDatabase(const std::string& database)
{
    try
    {
        db.release();
        db = std::make_unique<SQLite::Database>(database);

        queryShape = std::make_unique<SQLite::Statement>(*db, shapeSql.data());
        queryRules = std::make_unique<SQLite::Statement>(*db, rulesSql.data());
        queryShapeRelative = std::make_unique<SQLite::Statement>(*db, relativeShapeSql.data());
        queryRelativeRules= std::make_unique<SQLite::Statement>(*db, relativeRulesSql.data());
    }
    catch (const std::exception& e)
    {
        std::cerr << e.what();
        throw;
    }
}

void SQLiteCppWrapper::GetBid(int bidId, int& rank, int& suit)
{
    SQLite::Statement query(*db, "SELECT Rank, Suit, description FROM bids where id = ?");
    query.bind(1, bidId);

    if (query.executeStep())
    {
        rank = query.getColumn(0);
        suit = query.getColumn(1);
    }
}

std::tuple<int, Phase, std::string> SQLiteCppWrapper::GetRule(const HandCharacteristic& hand, const BoardCharacteristic& board, 
    const Phase& phase, int lastBidId, int position, const std::string& previousBidding, bool isCompetitive)
{
    try
    {
        // Bind parameters
        queryShape->reset();
        queryShape->bind(1, lastBidId);
        queryShape->bind(2, hand.suitLengths[0]);
        queryShape->bind(3, hand.suitLengths[1]);
        queryShape->bind(4, hand.suitLengths[2]);
        queryShape->bind(5, hand.suitLengths[3]);

        queryShape->bind(6, hand.Hcp);
        queryShape->bind(7, hand.isBalanced);
        queryShape->bind(8, (int)board.opponentsSuit);
        queryShape->bind(9, board.stopInOpponentsSuit);

        queryShape->bind(10, hand.lengthFirstSuit);
        queryShape->bind(11, hand.lengthSecondSuit);
        queryShape->bind(12, board.hasFit);
        queryShape->bind(13, board.fitIsMajor);
        queryShape->bind(14, modules);
        queryShape->bind(15, position);
        queryShape->bind(16, (int)phase);
        queryShape->bind(17, (std::string)previousBidding);
        queryShape->bind(18, isCompetitive);
        queryShape->bind(19, hand.isReverse);
        queryShape->bind(20, hand.isSemiBalanced);

        while (queryShape->executeStep())
        {
            auto id = queryShape->getColumn(5).getInt();
            auto bidId = queryShape->getColumn(0).isNull() ? 0 : queryShape->getColumn(0).getInt();
            auto nextPhase = queryShape->getColumn(3).isNull() ? phase : (Phase)queryShape->getColumn(3).getInt();
            auto str = queryShape->getColumn(4).getString();

            if (bidId != 0)
                return std::make_tuple(bidId, nextPhase, str);

            auto bidSuitKind = (BidKind)queryShape->getColumn(1).getInt();
            auto bidRank = queryShape->getColumn(2).getInt();
            auto relBidId = GetBidIdRelative(bidSuitKind, bidRank, lastBidId, hand, board);
            auto bidKindAuctionColumn = queryShape->getColumn(6);
            auto bidKindAuction = bidKindAuctionColumn.isNull() ? BidKindAuction::UnknownSuit : (BidKindAuction)bidKindAuctionColumn.getInt();
            if (relBidId != 0 && (bidKindAuction == BidKindAuction::UnknownSuit || bidKindAuction == GetBidKindFromAuction(previousBidding, relBidId)))
                return std::make_tuple(relBidId, nextPhase, str);
        }
        return std::make_tuple(0, phase, "");
    }
    catch (const std::exception& e)
    {
        std::cerr << e.what();
        throw;
    }
}

BidKindAuction SQLiteCppWrapper::GetBidKindFromAuction(const std::string& previousBidding, int bidId)
{
    //NonReverse,
    //    Reverse,
    //    OwnSuit,
    //    PartnersSuit,

    if (bidId <= 0)
        return BidKindAuction::UnknownSuit;

    auto bids = Utils::SplitAuction(previousBidding);
    auto lengthAuction = bids.size();

    if (IsRepondingToDouble(bids, lengthAuction))
        return BidKindAuction::RespondingToDouble;

    auto suit = Utils::GetSuitInt(bidId);
    if (HasFitWithPartner(bids, lengthAuction, suit))
        return BidKindAuction::PartnersSuit;

    if (IsRebidOwnSuit(bids, lengthAuction, suit))
        return BidKindAuction::OwnSuit;

    if (lengthAuction >= 4 && suit >= 0 && suit <= 3)
    {
        auto rank = Utils::GetRank(bidId);
        auto previousSuit = Utils::GetSuitInt(bids.at(lengthAuction - 4));
        auto previousRank = Utils::GetRank(bids.at(lengthAuction - 4));

        if (IsNonReverse(suit, rank, previousSuit, previousRank))
            return BidKindAuction::NonReverse;

        if (IsReverse(suit, rank, previousSuit, previousRank))
            return BidKindAuction::Reverse;
    }


    return BidKindAuction::UnknownSuit;
}

bool SQLiteCppWrapper::HasFitWithPartner(const std::vector<int>& bids, size_t lengthAuction, int suit)
{
    return HasFitWithPartnerPrevious(bids, lengthAuction, suit) || HasFitWithPartnerFirst(bids, lengthAuction, suit);
}
bool SQLiteCppWrapper::HasFitWithPartnerPrevious(const std::vector<int>& bids, size_t lengthAuction, int suit)
{
    return lengthAuction >= 2 && Utils::GetSuitInt(bids.at(lengthAuction - 2)) == suit;
}

bool SQLiteCppWrapper::HasFitWithPartnerFirst(const std::vector<int>& bids, size_t lengthAuction, int suit)
{
    return lengthAuction >= 6 && Utils::GetSuitInt(bids.at(lengthAuction - 6)) == suit;
}

bool SQLiteCppWrapper::IsReverse(int suit, int rank, int previousSuit, int previousRank)
{
    return (previousSuit <= 3 && previousSuit > suit && previousRank < rank);
}

bool SQLiteCppWrapper::IsNonReverse(int suit, int rank, int previousSuit, int previousRank)
{
    return (previousSuit <= 3 && (previousSuit < suit || previousRank > rank));
}

bool SQLiteCppWrapper::IsRebidOwnSuit(const std::vector<int>& bids, size_t lengthAuction, int suit)
{
    return (lengthAuction >= 4 && Utils::GetSuitInt(bids.at(lengthAuction - 4)) == suit);
}

bool SQLiteCppWrapper::IsRepondingToDouble(const std::vector<int>& bids, size_t lengthAuction)
{
    return (lengthAuction >= 2 && bids.at(lengthAuction - 2) == -1);
}

std::tuple<int, Phase, std::string> SQLiteCppWrapper::GetRelativeRule(const HandCharacteristic& hand, const BoardCharacteristic& boardCharacteristic, 
    int lastBidId, const std::string& previousBidding, bool allControlsPresent)
{
    try
    {
        // Bind parameters
        queryShapeRelative->reset();
        queryShapeRelative->bind(1, lastBidId);
        queryShapeRelative->bind(2, boardCharacteristic.keyCards);
        queryShapeRelative->bind(3, boardCharacteristic.trumpQueen);
        queryShapeRelative->bind(4, previousBidding);
        queryShapeRelative->bind(5, std::to_string(boardCharacteristic.fitWithPartnerSuit));
        queryShapeRelative->bind(6, hand.controls[0]);
        queryShapeRelative->bind(7, hand.controls[1]);
        queryShapeRelative->bind(8, hand.controls[2]);
        queryShapeRelative->bind(9, hand.controls[3]);
        queryShapeRelative->bind(10, allControlsPresent);
        queryShapeRelative->bind(11, GetLastBid(previousBidding));
        queryShapeRelative->bind(12, modules);

        while (queryShapeRelative->executeStep())
        {
            auto bidId = queryShapeRelative->getColumn(0).getInt();
            auto str = queryShapeRelative->getColumn(1).getString();

            if (bidId != 0)
                return std::make_tuple(bidId, Phase::SlamBidding, str);
        }
        return std::make_tuple(0, Phase::Unknown, "");
    }
    catch (const std::exception& e)
    {
        std::cerr << e.what();
        throw;
    }
}

std::string SQLiteCppWrapper::GetLastBid(const std::string& previousBidding)
{
    auto lastBid = previousBidding.length() == 0 ? previousBidding : previousBidding.substr(previousBidding.length() - 2);
    return lastBid == "NT" ? previousBidding.substr(previousBidding.length() - 3) : lastBid;
}

int SQLiteCppWrapper::GetBidIdRelative(BidKind bidSuitKind, int bidRank, int lastBidId, const HandCharacteristic& hand, const BoardCharacteristic& board)
{
    switch (bidSuitKind)
    {
    case BidKind::UnknownSuit:
        return 0;
    case BidKind::FirstSuit:
        return GetBidId(bidRank, IsNewSuit(hand.firstSuit, board.partnersSuits, board.opponentsSuit) ? hand.firstSuit : hand.secondSuit, lastBidId, hand.suitLengths);
    case BidKind::SecondSuit:
        return GetBidId(bidRank, hand.secondSuit, lastBidId, hand.suitLengths);
    case BidKind::LowestSuit:
        {
            auto bidId = GetBidId(bidRank, IsNewSuit(hand.lowestSuit, board.partnersSuits, board.opponentsSuit) ? hand.lowestSuit : hand.highestSuit, lastBidId, hand.suitLengths);
            return bidId != 0 ? bidId : GetBidId(bidRank, IsNewSuit(hand.lowestSuit, board.partnersSuits, board.opponentsSuit) ? hand.highestSuit : hand.lowestSuit, lastBidId, hand.suitLengths);
        }
    case BidKind::HighestSuit:
        return GetBidId(bidRank, IsNewSuit(hand.highestSuit, board.partnersSuits, board.opponentsSuit) ? hand.highestSuit : hand.lowestSuit, lastBidId, hand.suitLengths);
    case BidKind::PartnersSuit:
        return GetBidId(bidRank, board.fitWithPartnerSuit, lastBidId);
    default:
        throw new std::invalid_argument("Invalid value for bidSuitKind");
    }
}

bool SQLiteCppWrapper::IsNewSuit(int suit, const std::vector<int>& partnersSuits, int opponentsSuit)
{
    return (suit != -1 && partnersSuits.at(suit) == 0) && suit != opponentsSuit;
}

int SQLiteCppWrapper::GetBidId(int bidRank, int suit, int lastBidId, const std::vector<int>& suitLengths)
{
    if (suit == -1)
        return 0;
    if (suitLengths.size() > 0 && suitLengths.at(suit) < 4)
        return 0;
    return GetBidId(bidRank, suit, lastBidId);
}

int SQLiteCppWrapper::GetBidId(int bidRank, int suit, int lastBidId)
{
    if (suit == -1)
        return 0;
    auto bidId = (bidRank - 1) * 5 + (3 - suit) + 1;
    return bidId > lastBidId ? bidId : 0;
}

/// <summary>
/// Gets all the rules for this bid
/// TODO filter rules not applicable for this bid by using a different bidsuitkind
/// </summary>
/// <returns>a JSON string with all the rules</returns>
std::string SQLiteCppWrapper::GetRulesByBid(Phase phase, int bidId, int position, const std::string& previousBidding, bool isCompetitive)
{
    try
    {
        // Bind parameters
        queryRules->reset();
        queryRules->bind(1, bidId);
        queryRules->bind(2, bidId);
        queryRules->bind(3, modules);
        queryRules->bind(4, (int)phase);
        queryRules->bind(5, position);
        queryRules->bind(6, (std::string)previousBidding);
        queryRules->bind(7, isCompetitive);

        std::vector<std::unordered_map<std::string, std::string>> records;

        while (queryRules->executeStep())
        {
            if (!queryRules->getColumn("BidId").isNull() || (bidId % 5 != 0))
            {
                auto relevantIdsColumn = queryRules->getColumn("RelevantIds");
                if (!relevantIdsColumn.isNull())
                {
                    auto vectorOfIds = Utils::Split(relevantIdsColumn.getString(), ',');
                    if (std::find(vectorOfIds.begin(), vectorOfIds.end(), std::to_string(bidId)) == vectorOfIds.end())
                        continue;
                }

                auto bidRankColumn = queryRules->getColumn("BidRank");
                if (!bidRankColumn.isNull() && Utils::GetRank(bidId) != bidRankColumn.getInt())
                    continue;

                auto balancedColumn = queryRules->getColumn("IsBalanced");
                auto isBalanced = !balancedColumn.isNull() && (bool)balancedColumn.getInt();

                auto BidKindAuctionColumn = queryRules->getColumn("BidKindAuction");
                if (!BidKindAuctionColumn.isNull() && (BidKindAuction)BidKindAuctionColumn.getInt() != GetBidKindFromAuction(previousBidding, bidId))
                  continue;

                std::unordered_map<std::string, std::string> record;
                for (int i = 0; i < queryRules->getColumnCount() - 1; i++)
                {
                    auto column = queryRules->getColumn(i);
                    record.emplace(std::make_pair(column.getName(), isBalanced && column.getString() == "0" && (IsColumnMinSuit(column.getName())) ? "2" : column.getString()));
                }
                UpdateMinMax(bidId, record);
                records.push_back(record);
            }
        }
        nlohmann::json j = records;
        std::stringstream ss;
        ss << j;
        auto s = ss.str();
        return s;
    }
    catch (const std::exception& e)
    {
        std::cerr << e.what();
        throw;
    }
}

bool SQLiteCppWrapper::IsColumnMinSuit(const std::string& columnName)
{
    return columnName == "MinSpades" || columnName == "MinHearts" || columnName == "MinDimonds" || columnName == "MinClubs";
}

void SQLiteCppWrapper::UpdateMinMax(int bidId, std::unordered_map<std::string, std::string>& record)
{
    if (queryRules->getColumn("BidId").isNull() && (bidId % 5 != 0) && (bidId != -1) && !queryRules->getColumn("BidSuitKind").isNull())
    {
        auto suit = Utils::GetSuitPretty(bidId);
        auto bidKind = (BidKind)queryRules->getColumn("BidSuitKind").getInt();
        std::string suitKind = "";
        switch (bidKind)
        {
        case BidKind::FirstSuit:
        case BidKind::LowestSuit:
        case BidKind::HighestSuit:
        {
            record["Min" + suit] = queryRules->getColumn("MinFirstSuit").getString();
            record["Max" + suit] = queryRules->getColumn("MaxFirstSuit").getString();
        }
        break;
        case BidKind::SecondSuit:
        {
            record["Min" + suit] = queryRules->getColumn("MinSecondSuit").getString();
            record["Max" + suit] = queryRules->getColumn("MaxSecondSuit").getString();
        }
        break;
        case BidKind::PartnersSuit:
        {
            // TODO not technically correct, but it works
            record["Min" + suit] = "4";
            record["Max" + suit] = "13";
        }
        break;
        default:
            break;
        }

    }
}

/// <summary>
/// Gets all the rules for this bid in the relativeRules table
/// </summary>
/// <returns>a JSON string with all the relative rules</returns>
std::string SQLiteCppWrapper::GetRelativeRulesByBid(int bidId, const std::string& previousBidding)
{
    try
    {
        // Bind parameters
        queryRelativeRules->reset();
        queryRelativeRules->bind(1, bidId);
        queryRelativeRules->bind(2, previousBidding);
        queryRelativeRules->bind(3, GetLastBid(previousBidding));
        
        std::vector<std::unordered_map<std::string, std::string>> records;

        while (queryRelativeRules->executeStep())
        {
            if (!queryRelativeRules->getColumn("BidId").isNull() || (bidId % 5 != 0))
            {
                std::unordered_map<std::string, std::string> record;
                for (int i = 0; i < queryRelativeRules->getColumnCount() - 1; i++)
                {
                    auto column = queryRelativeRules->getColumn(i);
                    record.emplace(std::make_pair(column.getName(), column.getString()));
                }
                records.push_back(record);
            }
        }
        nlohmann::json j = records;
        std::stringstream ss;
        ss << j;
        auto s = ss.str();
        return s;
    }
    catch (const std::exception& e)
    {
        std::cerr << e.what();
        throw;
    }
}

void SQLiteCppWrapper::SetModules(int modules)
{
    this->modules = modules;
}
