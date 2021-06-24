#pragma once

#include "ISQLiteWrapper.h"
#include "SQLiteCpp/SQLiteCpp.h"
#include <unordered_map>

enum class BidKind;

class SQLiteCppWrapper : public ISQLiteWrapper
{
    constexpr static std::string_view shapeSql = R"(SELECT bidId, BidSuitKind, BidRank, NextPhase, Description FROM Rules 
        WHERE (bidId > ? OR bidId <= 0 OR bidID is NULL)
        AND ? BETWEEN MinSpades AND MaxSpades
        AND ? BETWEEN MinHearts AND MaxHearts
        AND ? BETWEEN MinDiamonds AND MaxDiamonds
        AND ? BETWEEN MinClubs AND MaxClubs
        AND ? BETWEEN MinHcp AND MaxHcp
        AND (IsBalanced IS NULL or IsBalanced = ?)
        AND (OpponentsSuit is NULL or OpponentsSuit = ?)
        AND (StopInOpponentsSuit is NULL or StopInOpponentsSuit = ?)
        AND ? BETWEEN MinFirstSuit AND MaxFirstSuit
        AND ? BETWEEN MinSecondSuit AND MaxSecondSuit
        AND (HasFit IS NULL or HasFit = ?)
        AND (FitIsMajor IS NULL or FitIsMajor = ?)
        AND Position = ?
        AND Phase = ?
        ORDER BY Priority ASC)";

    constexpr static std::string_view rulesSql = R"(SELECT * FROM Rules 
        WHERE (bidId = ? OR bidId is NULL)
        AND Phase = ?
        AND Position = ?)";


    std::unique_ptr<SQLite::Database> db;
    std::unique_ptr<SQLite::Statement> queryShape;
    std::unique_ptr<SQLite::Statement> queryRules;

public:
    SQLiteCppWrapper(const std::string& database);
private:
    void GetBid(int bidId, int& rank, int& suit) final;
    std::tuple<int, Phase, std::string> GetRule(const HandCharacteristic& hand, const BoardCharacteristic& boardCharacteristic, const Phase& phase, int lastBidId, int position) final;
    void SetDatabase(const std::string& database) override;
    std::string GetRulesByBid(Phase phase, int bidId, int position) final;
    void UpdateMinMax(int bidId, std::unordered_map<std::string, std::string>& record);
    std::string GetSuit(int bidId);
    int GetBidIdRelative(BidKind bidSuitKind, int bidRank, int lastBidId, const HandCharacteristic& hand, int partnersSuit, int opponentsSuit);
    bool IsNewSuit(int suit, int partnersSuit, int opponentsSuit);
    int GetBidId(int bidRank, int suit, int lastBidId, const std::vector<int>& suitLengths);
    int GetBidId(int bidRank, int suit, int lastBidId);

};

