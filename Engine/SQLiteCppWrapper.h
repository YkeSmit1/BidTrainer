#pragma once

#include "ISQLiteWrapper.h"
#include "SQLiteCpp/SQLiteCpp.h"

class SQLiteCppWrapper : public ISQLiteWrapper
{
    constexpr static std::string_view shapeSql = R"(SELECT bidId, NextPhase, Description FROM Rules 
        WHERE (bidId > ?)
        AND ? BETWEEN MinSpades AND MaxSpades
        AND ? BETWEEN MinHearts AND MaxHearts
        AND ? BETWEEN MinDiamonds AND MaxDiamonds
        AND ? BETWEEN MinClubs AND MaxClubs
        AND ? BETWEEN MinHcp AND MaxHcp
        AND (IsBalanced IS NULL or IsBalanced = ?)
        AND (IsReverse IS NULL or IsReverse = ?)
        AND Position = ?
        AND Phase = ?
        ORDER BY Priority ASC)";

    std::unique_ptr<SQLite::Database> db;
    std::unique_ptr<SQLite::Statement> queryShape;

public:
    SQLiteCppWrapper(const std::string& database);
private:
    void GetBid(int bidId, int& rank, int& suit) final;
    std::tuple<int, Phase, std::string> GetRule(const HandCharacteristic& hand, const Phase& phase, int lastBidId, int position) final;
    void SetDatabase(const std::string& database) override;
};

