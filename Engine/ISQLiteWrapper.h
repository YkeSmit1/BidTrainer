#pragma once

#include <tuple>
#include <string>
#include <vector>

struct HandCharacteristic;
struct BoardCharacteristic;
enum class Phase;

class ISQLiteWrapper  // NOLINT(hicpp-special-member-functions, cppcoreguidelines-special-member-functions)
{
public:
    virtual ~ISQLiteWrapper() = default;
    virtual std::tuple<int, Phase, std::string> GetRule(const HandCharacteristic& hand, const BoardCharacteristic& board, const Phase& phase, int lastBidId, int position) = 0;
    virtual void GetBid(int bidId, int& rank, int& suit) = 0;
    virtual std::string GetRulesByBid(Phase phase, int bidId, int position) = 0;
};

