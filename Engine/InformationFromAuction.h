#pragma once

#include <vector>
#include <string>
#include "Api.h"

struct InformationFromAuction
{
    std::vector<std::vector<int>> minSuitLengths{ std::vector<int>{0, 0, 0, 0}, std::vector<int>{0, 0, 0, 0}, std::vector<int>{0, 0, 0, 0}, std::vector<int>{0, 0, 0, 0} };
    std::vector<int> minHcps{ 0, 0, 0, 0 };
    Phase phase = Phase::Opening;
    std::vector<int> slamBidIds{};
    int bidIdBase = 0;
    std::vector<bool> controls{false, false, false, false};
    std::string previousSlamBidding{};
    bool isSlamBidding = false;
};

