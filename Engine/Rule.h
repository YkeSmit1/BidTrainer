#pragma once
#include <string>
#include <array>
#include <vector>

enum class Player { West, North, East, South };

struct HandCharacteristic
{
	std::string hand {};
	
	std::vector<int> suitLengths;
	std::string distribution;
	bool isBalanced = false;
	bool isReverse = false;
	bool isThreeSuiter = false;

	int Hcp = 0;

	static bool CalcuateIsReverse(const std::vector<int>& suitLengths);
	static bool CalcuateIsThreeSuiter(const std::vector<int>& suitLengths);
	static int CalculateHcp(const std::string& hand);
	static int NumberOfCards(const std::string& hand, char card);
	void Initialize(const std::string& hand);
	explicit HandCharacteristic(const std::string& hand);
	HandCharacteristic() = default;
};