#pragma once

enum class Phase
{
    Unknown,
    Opening,
    OneSuit,
    OneNT,
    Fit,
};

extern "C" __declspec(dllexport) int GetBidFromRule(Phase phase, const char* hand, int lastBidId, int position, Phase* newPhase, char* description);
extern "C" __declspec(dllexport) int Setup(const char* database);