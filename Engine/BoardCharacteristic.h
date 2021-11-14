#pragma once

#include <vector>
#include <string>

struct HandCharacteristic;


struct BoardCharacteristic
{
    bool hasFit;
    bool fitIsMajor;
    std::vector<int> partnersSuits;
    int fitWithPartnerSuit;
    int opponentsSuit;
    bool stopInOpponentsSuit;
    static BoardCharacteristic Create(HandCharacteristic hand, const std::vector<int>& partnersSuits, const std::vector<int>& opponentsSuits);
private:
    static int GetSuit(const std::vector<int>& suitLengths);
    static bool GetHasStopInOpponentsSuit(const std::string& hand, int opponentsSuit);
};

