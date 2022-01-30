#pragma once
#include <vector>
#include <string>
#include <sstream>

#define DBOUT( s )            \
{                             \
   std::ostringstream os_;    \
   os_ << s;                   \
   OutputDebugStringA( os_.str().c_str() );  \
}

class Utils
{
public:
    template<typename chartype>
    static std::vector<std::basic_string<chartype>> Split(const std::basic_string<chartype>& str, const chartype& delimeter);
    static std::string GetSuitPretty(int bidId);
    static std::string GetSuit(int suit);
    static std::string GetSuit2(int suit);
    static int GetSuitInt(int bidId);
    static int GetSuit(const std::string& suit);
    static std::string GetSuitASCII(int bidId);
    static std::vector<int> SplitAuction(const std::string& auction);
    static std::vector<std::string> SplitAuctionAsString(const std::string& auction);
    static int GetBidId(const std::string& bid);
    static int GetRank(int bidId);
    static int NumberOfCards(const std::string& hand, char card);
    static int CalculateHcp(const std::string& hand);
    static bool GetIsCompetitive(const std::string& bidding);
    static std::string GetBidASCII(int bidId);
    static int GetLastBidIdFromAuction(const std::string& bidding);
    static std::string GetLastBidFromAuction(const std::string& bidding);
};

template<typename chartype>
std::vector<std::basic_string<chartype>> Utils::Split(const std::basic_string<chartype>& str, const chartype& delimeter)
{
    std::vector<std::basic_string<chartype>> subStrings;
    std::basic_stringstream<chartype> stringStream(str);
    std::basic_string<chartype> item;
    while (getline(stringStream, item, delimeter))
    {
        subStrings.push_back(move(item));
    }

    while (subStrings.size() < 4)
        subStrings.emplace_back("");

    return subStrings;
}
