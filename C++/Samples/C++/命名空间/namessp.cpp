#include <iostream>
#include "namesp.h"
void other();
int main()
{
    using debts::Debt;
    Debt golf = {{"Benny", "Goatsniff"}, 120.0};
    showDebt(golf);
    other();

    return 0;
}

void other()
{
    using std::cout;
    using std::endl;
    using namespace debts;
    Person dg = {"Doodles", "Glister"};
    showPerson(dg);
    cout << endl;
}