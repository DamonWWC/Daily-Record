#include <string>
namespace pers
{
    struct Person
    {
        std::string fname;
        std::string lname;
    };
    void getPerson(Person &rp);
       void showPerson(const Person &rp);
}

namespace debts
{
    using namespace pers;
    struct Debt
    {
        Person name;
        double amount;
    };
    void showDebt(const Debt &rd);
    void getDebt(Debt &rd);
    double sumDebts(const Debt ar[], int n);
}