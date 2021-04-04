#include "pch.h"
#include "SQLiteCppWrapper.h"
#include <iostream>
#include "Rule.h"
#include <algorithm>
#include <vector>
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
std::tuple<int, Phase, std::string> SQLiteCppWrapper::GetRule(const HandCharacteristic& hand, const Phase& phase, int lastBidId, int position)
{
    try
    {
        // Bind parameters
        queryShape->reset();
        queryShape->bind(1, lastBidId);
        queryShape->bind(2, hand.Spades);
        queryShape->bind(3, hand.Hearts);
        queryShape->bind(4, hand.Diamonds);
        queryShape->bind(5, hand.Clubs);

        queryShape->bind(6, hand.Hcp);
        queryShape->bind(7, hand.isBalanced);
        queryShape->bind(8, hand.isReverse);
        queryShape->bind(9, position);
        queryShape->bind(10, (int)phase);

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
}