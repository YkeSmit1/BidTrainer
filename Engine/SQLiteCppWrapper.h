#pragma once

#include "ISQLiteWrapper.h"
#include "SQLiteCpp/SQLiteCpp.h"
#include <unordered_map>
#include "Api.h"

enum class BidKind;

class SQLiteCppWrapper : public ISQLiteWrapper
{
    constexpr static std::string_view shapeSql = R"(SELECT CASE 
                WHEN BidId IS NOT null THEN BidId
                WHEN BidId IS NULL AND BidSuitKind = 1 AND :firstSuit >= 0 THEN (BidRank - 1) * 5 + (3 - :firstSuit) + 1
                WHEN BidId IS NULL AND BidSuitKind = 2 AND :secondSuit >= 0 THEN (BidRank - 1) * 5 + (3 - :secondSuit) + 1
                WHEN BidId IS NULL AND BidSuitKind = 3 AND :lowestSuit >= 0 THEN (BidRank - 1) * 5 + (3 - :lowestSuit) + 1
                WHEN BidId IS NULL AND BidSuitKind = 4 AND :highestSuit >= 0 THEN (BidRank - 1) * 5 + (3 - :highestSuit) + 1
                WHEN BidId IS NULL AND BidSuitKind = 5 AND :fitWithPartnerSuit >= 0 THEN (BidRank - 1) * 5 + (3 - :fitWithPartnerSuit) + 1
                ELSE 0
            END,
            Description, Id, BidKindAuction, PreviousBidding, IsOpponentsSuit FROM Rules 
        WHERE (bidId > :lastBidId OR bidId <= 0 OR bidID is NULL)
        AND :minSpades BETWEEN MinSpades AND MaxSpades
        AND :minHearts BETWEEN MinHearts AND MaxHearts
        AND :minDiamonds BETWEEN MinDiamonds AND MaxDiamonds
        AND :minClubs BETWEEN MinClubs AND MaxClubs
        AND :minHcp BETWEEN MinHcp AND MaxHcp
        AND (IsBalanced IS NULL or IsBalanced = :isBalanced)
        AND (OpponentsSuit is NULL or OpponentsSuit = :opponentsSuit)
        AND (StopInOpponentsSuit is NULL or StopInOpponentsSuit = :stopInOpponentsSuit)
        AND :lengthFirstSuit BETWEEN MinFirstSuit AND MaxFirstSuit
        AND :lengthSecondSuit BETWEEN MinSecondSuit AND MaxSecondSuit
        AND (HasFit IS NULL or HasFit = :hasFit)
        AND (FitIsMajor IS NULL or FitIsMajor = :fitIsMajor)
        AND (Module IS NULL or :modules & Module = Module)
        AND Position = :position
        AND (IsCompetitive IS NULL or IsCompetitive = :isCompetitive)
        AND (IsReverse IS NULL or IsReverse = :isReverse)
        AND (IsSemiBalanced IS NULL or IsSemiBalanced = :isSemiBalanced)
        ORDER BY Priority ASC)";

    constexpr static std::string_view rulesSql = R"(SELECT PreviousBidding, * FROM Rules 
        WHERE ((bidId = ?) OR (bidId is NULL AND ? > 0))
        AND (Module IS NULL OR ? & Module = Module)
        AND Position = ?
        AND (IsCompetitive IS NULL OR IsCompetitive = ?)
        AND (BidRank IS NULL OR BidRank = ?)
        AND (BidKindAuction IS NULL OR BidKindAuction = ?)
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
    std::tuple<int, std::string> GetRule(const HandCharacteristic& hand, const BoardCharacteristic& boardCharacteristic, const std::string& previousBidding) final;
    std::tuple<int, std::string> GetRelativeRule(const HandCharacteristic& hand, const BoardCharacteristic& boardCharacteristic, const std::string& previousBidding) final;
    std::string GetRulesByBid(int bidId, const std::string& previousBidding) final;
    std::vector<std::unordered_map<std::string, std::string>> GetInternalRulesByBid(int bidId, const std::string& previousBidding) final;
    static bool HasFitWithPartnerPrevious(const std::vector<int>& bids, size_t lengthAuction, int suit);
    static bool HasFitWithPartnerFirst(const std::vector<int>& bids, size_t lengthAuction, int suit);
    static bool HasFitWithPartner(const std::vector<int>& bids, size_t lengthAuction, int suit);
    static bool IsReverse(int suit, int rank, int previousSuit, int previousRank);
    static bool IsNonReverse(int suit, int rank, int previousSuit, int previousRank);
    static bool IsRebidOwnSuit(const std::vector<int>& bids, size_t lengthAuction, int suit);
    void UpdateMinMax(int bidId, std::unordered_map<std::string, std::string>& record);
    std::string GetRelativeRulesByBid(int bidId, const std::string& previousBidding);
    std::vector<std::unordered_map<std::string, std::string>> GetInternalRelativeRulesByBid(int bidId, const std::string& previousBidding) final;
    void SetModules(int modules) override;
};