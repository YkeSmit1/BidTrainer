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

std::tuple<int, Phase, std::string> SQLiteCppWrapper::GetRule(const HandCharacteristic& hand, const BoardCharacteristic& board, const Phase& phase, int lastBidId, int position)
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
            auto relBidId = GetBidIdRelative(bidSuitKind, bidRank, lastBidId, hand, board.partnersSuit, board.opponentsSuit);
            if (relBidId != 0)
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

int SQLiteCppWrapper::GetBidIdRelative(BidKind bidSuitKind, int bidRank, int lastBidId, const HandCharacteristic& hand, int partnersSuit, int opponentsSuit)
{
    switch (bidSuitKind)
    {
    case BidKind::UnknownSuit:
        return 0;
    case BidKind::FirstSuit:
        return GetBidId(bidRank, IsNewSuit(hand.firstSuit, partnersSuit, opponentsSuit) ? hand.firstSuit : hand.secondSuit, lastBidId, hand.suitLengths);
    case BidKind::SecondSuit:
        return GetBidId(bidRank, hand.secondSuit, lastBidId, hand.suitLengths);
    case BidKind::LowestSuit:
        return GetBidId(bidRank, IsNewSuit(hand.lowestSuit, partnersSuit, opponentsSuit) ? hand.lowestSuit : hand.highestSuit, lastBidId, hand.suitLengths);
    case BidKind::HighestSuit:
        return GetBidId(bidRank, IsNewSuit(hand.highestSuit, partnersSuit, opponentsSuit) ? hand.highestSuit : hand.lowestSuit, lastBidId, hand.suitLengths);
    case BidKind::PartnersSuit:
        return GetBidId(bidRank, partnersSuit, lastBidId);
    default:
        throw new std::invalid_argument("Invalid value for bidSuitKind");
    }
}

bool SQLiteCppWrapper::IsNewSuit(int suit, int partnersSuit, int opponentsSuit)
{
    return suit != partnersSuit && suit != opponentsSuit;
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

void SQLiteCppWrapper::SetDatabase(const std::string& database)
{
    try
    {
        db.release();
        db = std::make_unique<SQLite::Database>(database);

        queryShape = std::make_unique<SQLite::Statement>(*db, shapeSql.data());
        queryRules = std::make_unique<SQLite::Statement>(*db, rulesSql.data());
    }
    catch (const std::exception& e)
    {
        std::cerr << e.what();
        throw;
    }
}

std::string SQLiteCppWrapper::GetRulesByBid(Phase phase, int bidId, int position)
{
    try
    {
        // Bind parameters
        queryRules->reset();
        queryRules->bind(1, bidId);
        queryRules->bind(2, modules);
        queryRules->bind(3, (int)phase);
        queryRules->bind(4, position);

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

                std::unordered_map<std::string, std::string> record;
                for (int i = 0; i < queryRules->getColumnCount() - 1; i++)
                {
                    auto column = queryRules->getColumn(i);
                    record.emplace(std::make_pair(column.getName(), column.getString()));
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

void SQLiteCppWrapper::UpdateMinMax(int bidId, std::unordered_map<std::string, std::string>& record)
{
    if (queryRules->getColumn("BidId").isNull() && (bidId % 5 != 0) && (bidId != -1) && !queryRules->getColumn("BidSuitKind").isNull())
    {
        auto suit = Utils::GetSuit(bidId);
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
        case BidKind::OpponentsSuit:
            // TODO
            break;

        default:
            break;
        }

    }
}

void SQLiteCppWrapper::SetModules(int modules)
{
    this->modules = modules;
}
