#pragma once

#include "ISQLiteWrapper.h"
#include "SQLiteCpp/SQLiteCpp.h"

class SQLiteCppWrapper : public ISQLiteWrapper
{
    constexpr static std::string_view shapeSql = R"(SELECT bidId, NextPhase, Description FROM Rules 
        WHERE (bidId > ? OR bidId <= 0)
        AND ? BETWEEN MinSpades AND MaxSpades
        AND ? BETWEEN MinHearts AND MaxHearts
        AND ? BETWEEN MinDiamonds AND MaxDiamonds
        AND ? BETWEEN MinClubs AND MaxClubs
        AND ? BETWEEN MinHcp AND MaxHcp
        AND (IsBalanced IS NULL or IsBalanced = ?)
        AND (IsTwoSuiter IS NULL or IsTwoSuiter = ?)
        AND (IsReverse IS NULL or IsReverse = ?)
        AND (FitSpades IS NULL or FitSpades = ?)
        AND (FitHearts IS NULL or FitHearts = ?)
        AND (FitDiamonds IS NULL or FitDiamonds = ?)
        AND (FitClubs IS NULL or FitClubs = ?)
        AND (OponentsSuit is NULL or OponentsSuit = ?)
        AND (StopInOponentsSuit is NULL or StopInOponentsSuit = ?)
        AND Position = ?
        AND Phase = ?
        ORDER BY Priority ASC)";

    constexpr static std::string_view rulesSql = R"(SELECT * FROM Rules 
        WHERE bidId = ?
        AND Phase = ?
        AND Position = ?)";


    std::unique_ptr<SQLite::Database> db;
    std::unique_ptr<SQLite::Statement> queryShape;
    std::unique_ptr<SQLite::Statement> queryRules;

public:
    SQLiteCppWrapper(const std::string& database);
private:
    void GetBid(int bidId, int& rank, int& suit) final;
    std::tuple<int, Phase, std::string> GetRule(const HandCharacteristic& hand, std::vector<bool>& fits, const int oponentsSuit,
        bool stopInOponentsSuit, const Phase& phase, int lastBidId, int position) final;
    void SetDatabase(const std::string& database) override;
    std::string GetRulesByBid(Phase phase, int bidId, int position) final;
};

