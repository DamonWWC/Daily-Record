#progma once

#include <iostream>

using namespace std;

class Cow{
    private:
    char name[20];
    char * hobby;
    double weight;
    public:
    Cow(){};
    Cow(const char * nm, const char * ho, double wt);
    Cow(const Cow & c);
    ~Cow();
    Cow & operator=(const Cow & c);
    void ShowCow() const;

}
