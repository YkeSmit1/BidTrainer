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
	bool isTwoSuiter = false;

	int Hcp = 0;

	static bool CalculateIsReverse(const std::vector<int>& suitLengths);
	static bool CalculateIsTwoSuiter(const std::vector<int>& suitLengths);
	static bool CalculateIsThreeSuiter(const std::vector<int>& suitLengths);
	static int CalculateHcp(const std::string& hand);
	static int NumberOfCards(const std::string& hand, char card);
	void Initialize(const std::string& hand);
	explicit HandCharacteristic(const std::string& hand);
	HandCharacteristic() = default;
};