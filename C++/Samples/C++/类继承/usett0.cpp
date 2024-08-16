#include <iostream>
#include "tabtenn0.h"

int main()
{
    using std::cout;
    TableTennisPlayer player1("Tara", "Boomdea", false);
    TableTennisPlayer player2("Sonny", "Chin", true);
    player1.Name();
    if (player1.HasTable())
        cout << ": has a table.\n";
    else
        cout << ": hasn't a table.\n";
    player2.Name();
    if (player2.HasTable())
        cout << ": has a table.\n";
    else
        cout << ": hasn't a table.\n";

    return 0;
}