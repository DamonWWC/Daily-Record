#include <iostream>
#include "Stock00.cpp"
int main()
{

    using namespace std;
    // Stock fluffy_the_cat;
    // fluffy_the_cat.acquire("NanoSmart", 20, 12.50);
    // fluffy_the_cat.show();
    // fluffy_the_cat.buy(15, 18.25);
    // fluffy_the_cat.show();
    // fluffy_the_cat.sell(400, 20.00);
    // fluffy_the_cat.show();
    // fluffy_the_cat.buy(30000, 40.25);
    // fluffy_the_cat.show();
    // fluffy_the_cat.sell(20000, 20.00);
    // fluffy_the_cat.show();

    Stock stocks[4] = {
        Stock("NanoSmart", 12, 20.0),
        Stock("Boffo Objects", 200, 2.0),
        Stock("Monolithic Obelisks", 130, 3.25),
        Stock("Fleep Enterprises", 60, 6.5)};
    std::cout << "Stock holdings:\n";
    int st;
    for (st = 0; st < 4; st++)
    {
        stocks[st].show();
    }
    const Stock *top = &stocks[0];
    for (st = 1; st < 4; st++)
        top = &top->topval(stocks[st]);
    std::cout << "\nMost valuable holding:\n";
    top->show();

    return 0;
}