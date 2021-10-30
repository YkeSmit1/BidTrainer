#pragma once

#include "ISQLiteWrapper.h"
#include "SQLiteCpp/SQLiteCpp.h"
#include <unordered_map>
#include "Api.h"

enum class BidKind;

class SQLiteCppWrapper : public ISQLiteWrapper
{
    constexpr static std::string_view shapeSql = R"(SELECT bidId, BidSuitKind, BidRank, NextPhase, Description, Id FROM Rules 
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
        AND (Module IS NULL or ? & Module = Module)
        AND Position = ?
        AND Phase = ?
        AND (PreviousBidding IS NULL or PreviousBidding = ?)
        AND (IsCompetitive IS NULL or IsCompetitive = ?)
        ORDER BY Priority ASC)";

    constexpr static std::string_view rulesSql = R"(SELECT * FROM Rules 
        WHERE ((bidId = ?) OR (bidId is NULL AND ? > 0))
        AND (Module IS NULL or ? & Module = Module)
        AND Phase = ?
        AND Position = ?
        AND (PreviousBidding IS NULL or PreviousBidding = ?)
        AND (IsCompetitive IS NULL or IsCompetitive = ?))";

    std::unique_ptr<SQLite::Database> db;
    std::unique_ptr<SQLite::Statement> queryShape;
    std::unique_ptr<SQLite::Statement> queryRules;

    int modules = (int)Modules::FiveCardMajors;

public:
    SQLiteCppWrapper(const std::string& database);
private:
    void GetBid(int bidId, int& rank, int& suit) final;
    std::tuple<int, Phase, std::string> GetRule(const HandCharacteristic& hand, const BoardCharacteristic& boardCharacteristic, 
        const Phase& phase, int lastBidId, int position, const std::string& previousBidding, bool isCompetitive) final;
    void SetDatabase(const std::string& database) override;
    std::string GetRulesByBid(Phase phase, int bidId, int position, const std::string& previousBidding, bool isCompetitive) final;
    bool IsColumnMinSuit(const std::string& columnName);
    void UpdateMinMax(int bidId, std::unordered_map<std::string, std::string>& record);
    int GetBidIdRelative(BidKind bidSuitKind, int bidRank, int lastBidId, const HandCharacteristic& hand, int partnersSuit, int opponentsSuit);
    bool IsNewSuit(int suit, int partnersSuit, int opponentsSuit);
    int GetBidId(int bidRank, int suit, int lastBidId, const std::vector<int>& suitLengths);
    int GetBidId(int bidRank, int suit, int lastBidId);
    void SetModules(int modules) override;
};