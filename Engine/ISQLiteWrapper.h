#pragma once

#include <tuple>
#include <string>
#include <vector>
#include <unordered_map>

struct HandCharacteristic;
struct BoardCharacteristic;
enum class Phase;

class ISQLiteWrapper  // NOLINT(hicpp-special-member-functions, cppcoreguidelines-special-member-functions)
{
public:
    virtual ~ISQLiteWrapper() = default;
    virtual std::tuple<int, Phase, std::string> GetRule(const HandCharacteristic& hand, const BoardCharacteristic& board, 
        const Phase& phase, const std::string& previousBidding) = 0;
    virtual void GetBid(int bidId, int& rank, int& suit) = 0;
    virtual std::tuple<int, Phase, std::string> GetRelativeRule(const HandCharacteristic& hand, const BoardCharacteristic& board,
        const std::string& previousBidding) = 0;
    virtual void SetDatabase(const std::string& database) = 0;
    virtual std::string GetRulesByBid(Phase phase, int bidId, int position, const std::string& previousBidding, bool isCompetitive) = 0;
    virtual std::vector<std::unordered_map<std::string, std::string>> GetInternalRulesByBid(Phase phase, int bidId, int position, const std::string& previousBidding, bool isCompetitive) = 0;
    virtual std::string GetRelativeRulesByBid(int bidId, const std::string& previousBidding) = 0;
    virtual std::vector<std::unordered_map<std::string, std::string>> GetInternalRelativeRulesByBid(int bidId, const std::string& previousBidding) = 0;
    virtual void SetModules(int modules) = 0;
};

