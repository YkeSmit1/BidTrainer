#include "pch.h"
#include "../Engine/Rule.h"
#include "../Engine/BoardCharacteristic.h"
#include "../Engine/SQLiteCppWrapper.h"

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

    handCharacteristic.Initialize("432,VB2,A,AK5432");
    EXPECT_EQ(handCharacteristic.controls[0], false);
    EXPECT_EQ(handCharacteristic.controls[1], false);
    EXPECT_EQ(handCharacteristic.controls[2], true);
    EXPECT_EQ(handCharacteristic.controls[3], true);

    handCharacteristic.Initialize("A65432,K65432,2,");
    EXPECT_EQ(handCharacteristic.controls[0], true);
    EXPECT_EQ(handCharacteristic.controls[1], true);
    EXPECT_EQ(handCharacteristic.controls[2], true);
    EXPECT_EQ(handCharacteristic.controls[3], true);
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
    EXPECT_EQ(boardCharacteristic.keyCards, 2);
    EXPECT_EQ(boardCharacteristic.trumpQueen, false);

    HandCharacteristic handCharacteristicSlam{};
    handCharacteristicSlam.Initialize("A432,Q5432,A,AJ4");
    boardCharacteristic = BoardCharacteristic::Create(handCharacteristicSlam, { 0, 4, 0, 0 }, { 4, 0, 0, 0 });
    EXPECT_EQ(boardCharacteristic.hasFit, true);
    EXPECT_EQ(boardCharacteristic.fitIsMajor, true);
    EXPECT_EQ(boardCharacteristic.opponentsSuit, 0);
    EXPECT_EQ(boardCharacteristic.fitWithPartnerSuit, 1);
    EXPECT_EQ(boardCharacteristic.stopInOpponentsSuit, true);
    EXPECT_EQ(boardCharacteristic.keyCards, 3);
    EXPECT_EQ(boardCharacteristic.trumpQueen, true);
}

TEST(TestGetBidKindFromAuction, TestName)
{
    EXPECT_EQ((int)SQLiteCppWrapper::GetBidKindFromAuction("", 4), (int)BidKindAuction::UnknownSuit);

    EXPECT_EQ((int)SQLiteCppWrapper::GetBidKindFromAuction("1H", 4), (int)BidKindAuction::UnknownSuit);

    EXPECT_EQ((int)SQLiteCppWrapper::GetBidKindFromAuction("1HPass", 4), (int)BidKindAuction::UnknownSuit);
    EXPECT_EQ((int)SQLiteCppWrapper::GetBidKindFromAuction("1HX", 5), (int)BidKindAuction::UnknownSuit);
    EXPECT_EQ((int)SQLiteCppWrapper::GetBidKindFromAuction("1HPass", 8), (int)BidKindAuction::PartnersSuit);

    EXPECT_EQ((int)SQLiteCppWrapper::GetBidKindFromAuction("1H1SPass", 5), (int)BidKindAuction::UnknownSuit);
    EXPECT_EQ((int)SQLiteCppWrapper::GetBidKindFromAuction("1H1SX", 6), (int)BidKindAuction::UnknownSuit);
    EXPECT_EQ((int)SQLiteCppWrapper::GetBidKindFromAuction("1H1SPass", 9), (int)BidKindAuction::PartnersSuit);

    EXPECT_EQ((int)SQLiteCppWrapper::GetBidKindFromAuction("1DPass1SPass", 5), (int)BidKindAuction::UnknownSuit);
    EXPECT_EQ((int)SQLiteCppWrapper::GetBidKindFromAuction("1DPass1SPass", 6), (int)BidKindAuction::NonReverse);
    EXPECT_EQ((int)SQLiteCppWrapper::GetBidKindFromAuction("1DPass1SPass", 7), (int)BidKindAuction::OwnSuit);
    EXPECT_EQ((int)SQLiteCppWrapper::GetBidKindFromAuction("1DPass1SPass", 8), (int)BidKindAuction::Reverse);
    EXPECT_EQ((int)SQLiteCppWrapper::GetBidKindFromAuction("1DPass1SPass", 9), (int)BidKindAuction::PartnersSuit);

    EXPECT_EQ((int)SQLiteCppWrapper::GetBidKindFromAuction("1DPass1HPass1NTPass", 6), (int)BidKindAuction::NonReverse);
    EXPECT_EQ((int)SQLiteCppWrapper::GetBidKindFromAuction("1DPass1HPass1NTPass", 7), (int)BidKindAuction::PartnersSuit);
    EXPECT_EQ((int)SQLiteCppWrapper::GetBidKindFromAuction("1DPass1HPass1NTPass", 8), (int)BidKindAuction::OwnSuit);
    EXPECT_EQ((int)SQLiteCppWrapper::GetBidKindFromAuction("1DPass1HPass1NTPass", 9), (int)BidKindAuction::Reverse);
    EXPECT_EQ((int)SQLiteCppWrapper::GetBidKindFromAuction("1DPass1HPass2SPass", 14), (int)BidKindAuction::PartnersSuit);
}