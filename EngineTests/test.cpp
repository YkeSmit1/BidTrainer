#include "pch.h"
#include "../Engine/Rule.h"
#include "../Engine/BoardCharacteristic.h"

TEST(TestHandCharacteristic, TestName)
{
    HandCharacteristic handCharacteristic{};
    handCharacteristic.Initialize("A432,K5432,Q,J43");

    EXPECT_EQ(handCharacteristic.isBalanced, false);
    EXPECT_EQ(handCharacteristic.isSemiBalanced, true);
    EXPECT_EQ(handCharacteristic.suitLengths[0], 4);
    EXPECT_EQ(handCharacteristic.suitLengths[1], 5);
    EXPECT_EQ(handCharacteristic.suitLengths[2], 1);
    EXPECT_EQ(handCharacteristic.suitLengths[3], 3);
    EXPECT_EQ(handCharacteristic.lengthFirstSuit, 5);
    EXPECT_EQ(handCharacteristic.lengthSecondSuit, 4);
    EXPECT_EQ(handCharacteristic.firstSuit, 1);
    EXPECT_EQ(handCharacteristic.secondSuit, 0);
    EXPECT_EQ(handCharacteristic.lowestSuit, -1);
    EXPECT_EQ(handCharacteristic.highestSuit, -1);
    EXPECT_EQ(handCharacteristic.Hcp, 10);

    handCharacteristic.Initialize("A32,K432,K432,J3");

    EXPECT_EQ(handCharacteristic.isBalanced, true);
    EXPECT_EQ(handCharacteristic.isSemiBalanced, true);
    EXPECT_EQ(handCharacteristic.suitLengths[0], 3);
    EXPECT_EQ(handCharacteristic.suitLengths[1], 4);
    EXPECT_EQ(handCharacteristic.suitLengths[2], 4);
    EXPECT_EQ(handCharacteristic.suitLengths[3], 2);
    EXPECT_EQ(handCharacteristic.lengthFirstSuit, 4);
    EXPECT_EQ(handCharacteristic.lengthSecondSuit, 4);
    EXPECT_EQ(handCharacteristic.firstSuit, -1);
    EXPECT_EQ(handCharacteristic.secondSuit, -1);
    EXPECT_EQ(handCharacteristic.lowestSuit, 2);
    EXPECT_EQ(handCharacteristic.highestSuit, 1);
    EXPECT_EQ(handCharacteristic.Hcp, 11);

    handCharacteristic.Initialize("A,K432,J432,K432");
    EXPECT_EQ(handCharacteristic.lowestSuit, 3);
    EXPECT_EQ(handCharacteristic.highestSuit, 1);

    handCharacteristic.Initialize("K432,A,J432,K432");
    EXPECT_EQ(handCharacteristic.lowestSuit, 3);
    EXPECT_EQ(handCharacteristic.highestSuit, 0);

    handCharacteristic.Initialize("K432,J432,K432,A");
    EXPECT_EQ(handCharacteristic.lowestSuit, 2);
    EXPECT_EQ(handCharacteristic.highestSuit, 0);

    handCharacteristic.Initialize("A32,K432,K432,J3");
    EXPECT_EQ(handCharacteristic.isReverse, false);
    handCharacteristic.Initialize("A2,K5432,K432,J3");
    EXPECT_EQ(handCharacteristic.isReverse, false);
    handCharacteristic.Initialize("A2,K432,K5432,J3");
    EXPECT_EQ(handCharacteristic.isReverse, true);
    handCharacteristic.Initialize("A,K5432,K65432,J");
    EXPECT_EQ(handCharacteristic.isReverse, true);
    EXPECT_EQ(handCharacteristic.isSemiBalanced, false);
}

TEST(TestBoardCharacteristic, TestName) 
{
    HandCharacteristic handCharacteristic{};
    handCharacteristic.Initialize("A432,K5432,Q,J43");

    BoardCharacteristic boardCharacteristic = BoardCharacteristic::Create(handCharacteristic, { 0, 0, 0, 0 }, { 0, 0, 0, 0 });
    EXPECT_EQ(boardCharacteristic.hasFit, false);
    EXPECT_EQ(boardCharacteristic.fitIsMajor, false);
    EXPECT_EQ(boardCharacteristic.opponentsSuit, -1);
    EXPECT_EQ(boardCharacteristic.fitWithPartnerSuit, -1);
    EXPECT_EQ(boardCharacteristic.stopInOpponentsSuit, false);

    boardCharacteristic = BoardCharacteristic::Create(handCharacteristic, { 0, 0, 0, 4 }, { 0, 0, 4, 0 });
    EXPECT_EQ(boardCharacteristic.hasFit, false);
    EXPECT_EQ(boardCharacteristic.fitIsMajor, false);
    EXPECT_EQ(boardCharacteristic.opponentsSuit, 2);
    EXPECT_EQ(boardCharacteristic.fitWithPartnerSuit, -1);
    EXPECT_EQ(boardCharacteristic.stopInOpponentsSuit, false);

    boardCharacteristic = BoardCharacteristic::Create(handCharacteristic, { 0, 4, 0, 0 }, { 4, 0, 0, 0 });
    EXPECT_EQ(boardCharacteristic.hasFit, true);
    EXPECT_EQ(boardCharacteristic.fitIsMajor, true);
    EXPECT_EQ(boardCharacteristic.opponentsSuit, 0);
    EXPECT_EQ(boardCharacteristic.fitWithPartnerSuit, 1);
    EXPECT_EQ(boardCharacteristic.stopInOpponentsSuit, true);
}