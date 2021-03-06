#include "pch.h"
#include "SQLiteOrmWrapper.h"
#include <sqlite_orm/sqlite_orm.h>
#include "Rule.h"
#include <iostream>

using namespace sqlite_orm;
using namespace std;

void SQLiteOrmWrapper::GetBid(int, int& rank, int& suit)
{
    rank = 0;
    suit = 0;
    return;
}

SQLiteOrmWrapper::SQLiteOrmWrapper()
{
}

std::tuple<int, bool, std::string> SQLiteOrmWrapper::GetRule(const HandCharacteristic& handCharacteristic, const Fase& fase, int lastBidId)
{
    auto storage = make_storage("Tosr.db3",
        make_table("Rules",
            make_column("Id", &Rule::id, autoincrement(), primary_key()),
            make_column("BidId", &Rule::bidId),
            make_column("FaseId", &Rule::faseId),
            make_column("NextFaseId", &Rule::nextFaseId),
            make_column("MinSpades", &Rule::minSpades),
            make_column("MaxSpades", &Rule::maxSpades),
            make_column("MinHearts", &Rule::minHearts),
            make_column("MaxHearts", &Rule::maxHearts),
            make_column("MinDiamonds", &Rule::minDiamonds),
            make_column("MaxDiamonds", &Rule::maxDiamonds),
            make_column("MinClubs", &Rule::minClubs),
            make_column("MaxClubs", &Rule::maxClubs),
            make_column("Distribution", &Rule::distribution),
            make_column("MinControls", &Rule::minControls),
            make_column("MaxControls", &Rule::maxControls),
            make_column("IsBalanced", &Rule::isBalanced)
            ));
    storage.pragma.journal_mode(journal_mode::DELETE);

    auto rows = storage.select(&Rule::bidId, 
        where(c(&Rule::minSpades) <= handCharacteristic.Spades
        and c(&Rule::maxSpades) >= handCharacteristic.Spades
        and c(&Rule::minHearts) <= handCharacteristic.Hearts
        and c(&Rule::maxHearts) >= handCharacteristic.Hearts
        and c(&Rule::minDiamonds) <= handCharacteristic.Diamonds
        and c(&Rule::maxDiamonds) >= handCharacteristic.Diamonds
        and c(&Rule::minClubs) <= handCharacteristic.Clubs
        and c(&Rule::maxClubs) >= handCharacteristic.Clubs
        and c(&Rule::minControls) <= handCharacteristic.Controls
        and c(&Rule::maxControls) >= handCharacteristic.Controls
        and (c(&Rule::distribution) == handCharacteristic.distribution or is_null(&Rule::distribution))
        and (c(&Rule::isBalanced) == handCharacteristic.isBalanced or is_null(&Rule::isBalanced))));
    //std::remove_if(rows.begin(), rows.end(), [](std::tuple<int, int, std::string, bool> row){return std::get< row.})

   std::sort(rows.begin(), rows.end());

   std::string emptystring;

    return std::make_tuple(rows.front(), false, emptystring);
}

void SQLiteOrmWrapper::SetDatabase(const std::string& database)
{
}
