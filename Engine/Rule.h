#pragma once
#include <string>
#include <unordered_map>
#include <array>
#include <vector>

enum class Player { West, North, East, South };

struct HandCharacteristic
{
	std::string hand {};
	
	int Spades = 0;
	int Hearts = 0;
	int Diamonds = 0;
	int Clubs = 0;

	std::string distribution;
	bool isBalanced = false;
	bool isReverse = false;
	bool isThreeSuiter = false;

	int Hcp = 0;

	static bool CalcuateIsReverse(const std::unordered_map<int, size_t>& suitLength);
	static bool CalcuateIsThreeSuiter(const std::unordered_map<int, size_t>& suitLength);
	static int CalculateHcp(const std::string& hand);
	static int NumberOfCards(const std::string& hand, char card);
	void Initialize(const std::string& hand);
	explicit HandCharacteristic(const std::string& hand);
	HandCharacteristic() = default;
};


class Rule
{
public :
	int id;
	int bidId;
	int faseId;
	int nextFaseId;

	int minSpades;
	int maxSpades;
	int minHearts;
	int maxHearts;
	int minDiamonds;
	int maxDiamonds;
	int minClubs;
	int maxClubs;

	std::string distribution;
	int minControls;
	int maxControls;

	bool isBalanced;
};
