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

std::tuple<int, Phase, std::string> SQLiteCppWrapper::GetRule(const HandCharacteristic& hand, std::vector<bool>& fits, 
    const int oponentsSuit, bool stopInOponentsSuit, const Phase& phase, int lastBidId, int position)
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
        queryShape->bind(8, hand.isTwoSuiter);
        queryShape->bind(9, hand.isReverse);

        queryShape->bind(10, fits[0]);
        queryShape->bind(11, fits[1]);
        queryShape->bind(12, fits[2]);
        queryShape->bind(13, fits[3]);
        queryShape->bind(14, (int)oponentsSuit);
        queryShape->bind(15, stopInOponentsSuit);
        queryShape->bind(16, position);
        queryShape->bind(17, (int)phase);

        if (!queryShape->executeStep())
            return std::make_tuple(0, phase, "");

        int bidId = queryShape->getColumn(0).getInt();
        auto nextPhase = queryShape->getColumn(1);
        auto str = queryShape->getColumn(2).getString();

        return std::make_tuple(bidId, nextPhase.isNull() ? phase : (Phase)nextPhase.getInt(), str);
    }
    catch (const std::exception& e)
    {
        std::cerr << e.what();
        throw;
    }
}

void SQLiteCppWrapper::SetDatabase(const std::string& database)
{
    db.release();
    db = std::make_unique<SQLite::Database>(database);

    queryShape = std::make_unique<SQLite::Statement>(*db, shapeSql.data());
    queryRules = std::make_unique<SQLite::Statement>(*db, rulesSql.data());
}

std::string SQLiteCppWrapper::GetRulesByBid(Phase phase, int bidId, int position)
{
    using nlohmann::json;

    try
    {
        // Bind parameters
        queryRules->reset();
        queryRules->bind(1, bidId);
        queryRules->bind(2, (int)phase);
        queryRules->bind(3, position);

        std::vector<std::unordered_map<std::string, std::string>> records;

        while (queryRules->executeStep())
        {
            std::unordered_map<std::string, std::string> record;
            for (int i = 0; i < queryRules->getColumnCount() - 1; i++)
            {
                auto column = queryRules->getColumn(i);
                record.emplace(std::make_pair(column.getName(), column.getString()));
            }
            records.push_back(record);
        }
        json j = records;
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
