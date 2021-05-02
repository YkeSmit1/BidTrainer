#pragma once

#include <tuple>
#include <string>
#include <vector>

struct HandCharacteristic;
enum class Phase;

class ISQLiteWrapper  // NOLINT(hicpp-special-member-functions, cppcoreguidelines-special-member-functions)
{
public:
    virtual ~ISQLiteWrapper() = default;
    virtual std::tuple<int, Phase, std::string> GetRule(const HandCharacteristic& handCharacteristic, std::vector<bool>& fits, 
        const int oponentsSuit, bool stopInOponentsSuit, const Phase& phase, int lastBidId, int position) = 0;
    virtual void GetBid(int bidId, int& rank, int& suit) = 0;
    virtual void SetDatabase(const std::string& database) = 0;
    virtual std::string GetRulesByBid(Phase phase, int bidId, int position) = 0;
};

