#pragma once

#include "ISQLiteWrapper.h"
#include "SQLiteCpp/SQLiteCpp.h"
#include <unordered_map>
#include "Api.h"

enum class BidKind;

class SQLiteCppWrapper : public ISQLiteWrapper
{
    constexpr static std::string_view shapeSql = R"(SELECT bidId, BidSuitKind, BidRank, NextPhase, Description, Id, BidKindAuction FROM Rules 
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
        AND (IsReverse IS NULL or IsReverse = ?)
        AND (IsSemiBalanced IS NULL or IsSemiBalanced = ?)
        ORDER BY Priority ASC)";

    constexpr static std::string_view rulesSql = R"(SELECT * FROM Rules 
        WHERE ((bidId = ?) OR (bidId is NULL AND ? > 0))
        AND (Module IS NULL or ? & Module = Module)
        AND Phase = ?
        AND Position = ?
        AND (PreviousBidding IS NULL or PreviousBidding = ?)
        AND (IsCompetitive IS NULL or IsCompetitive = ?)
        AND UseInCalculation IS NULL)";

    constexpr static std::string_view relativeShapeSql = R"(SELECT bidId, Description FROM RelativeRules 
        WHERE bidId > ?
        AND (KeyCards IS NULL or KeyCards = ?)
        AND (TrumpQueen IS NULL or TrumpQueen = ?)
        AND (PreviousBidding IS NULL or PreviousBidding = ?)
        AND (TrumpSuits IS NULL OR TrumpSuits LIKE '%' || ? || '%')
        AND (SpadeControl IS NULL or SpadeControl = ?)
        AND (HeartControl IS NULL or HeartControl = ?)
        AND (DiamondControl IS NULL or DiamondControl = ?)
        AND (ClubControl IS NULL or ClubControl = ?)
        AND (AllControlsPresent IS NULL or AllControlsPresent = ?)
        AND (LastBid IS NULL or LastBid = ?)
        AND (Module IS NULL or ? & Module = Module)
        ORDER BY Priority ASC)";

    constexpr static std::string_view relativeRulesSql = R"(SELECT * FROM RelativeRules 
        WHERE bidId = ?
        AND (PreviousBidding IS NULL or PreviousBidding = ?)
        AND (LastBid IS NULL or LastBid = ?))";

    std::unique_ptr<SQLite::Database> db;
    std::unique_ptr<SQLite::Statement> queryShape;
    std::unique_ptr<SQLite::Statement> queryRules;
    std::unique_ptr<SQLite::Statement> queryShapeRelative;
    std::unique_ptr<SQLite::Statement> queryRelativeRules;

    int modules = (int)Modules::FiveCardMajors;

public:
    SQLiteCppWrapper(const std::string& database);
    static BidKindAuction GetBidKindFromAuction(const std::string& previousBidding, int bidId);
private:
    void GetBid(int bidId, int& rank, int& suit) final;
    std::tuple<int, Phase, std::string> GetRule(const HandCharacteristic& hand, const BoardCharacteristic& boardCharacteristic, 
        const Phase& phase, int lastBidId, int position, const std::string& previousBidding, bool isCompetitive) final;
    std::tuple<int, Phase, std::string> GetRelativeRule(const HandCharacteristic& hand, const BoardCharacteristic& boardCharacteristic,
        int lastBidId, const std::string& previousBidding, bool allControlsPresent) final;
    std::string GetLastBid(const std::string& previousBidding);
    void SetDatabase(const std::string& database) override;
    std::string GetRulesByBid(Phase phase, int bidId, int position, const std::string& previousBidding, bool isCompetitive) final;
    static bool HasFitWithPartnerPrevious(const std::vector<int>& bids, size_t lengthAuction, int suit);
    static bool HasFitWithPartnerFirst(const std::vector<int>& bids, size_t lengthAuction, int suit);
    static bool HasFitWithPartner(const std::vector<int>& bids, size_t lengthAuction, int suit);
    static bool IsReverse(int suit, int rank, int previousSuit, int previousRank);
    static bool IsNonReverse(int suit, int rank, int previousSuit, int previousRank);
    static bool IsRebidOwnSuit(const std::vector<int>& bids, size_t lengthAuction, int suit);
    static bool IsRepondingToDouble(const std::vector<int>& bids, size_t lengthAuction);
    bool IsColumnMinSuit(const std::string& columnName);
    void UpdateMinMax(int bidId, std::unordered_map<std::string, std::string>& record);
    int GetBidIdRelative(BidKind bidSuitKind, int bidRank, int lastBidId, const HandCharacteristic& hand, const BoardCharacteristic& board);
    bool IsNewSuit(int suit, const std::vector<int>& partnersSuits, int opponentsSuit);
    int GetBidId(int bidRank, int suit, int lastBidId, const std::vector<int>& suitLengths);
    int GetBidId(int bidRank, int suit, int lastBidId);
    std::string GetRelativeRulesByBid(int bidId, const std::string& previousBidding);
    void SetModules(int modules) override;
};