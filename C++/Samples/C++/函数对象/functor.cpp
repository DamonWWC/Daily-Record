#include <iostream>
#include <list>
#include <iterator>
#include <algorithm>

using namespace std;

template <class T>
class TooBig
{
private:
    T cutoff;

public:
    TooBig(const T &t) : cutoff(t) {}
    bool operator()(const T &x)
    {
        return x > cutoff;
    }
};
void outint(int n)
{
    std::cout << n << " ";
}

int main()
{
    TooBig<int> f100(100);
    int vals[10] = {50, 100, 90, 150, 120, 70, 130, 110, 80, 140};
    // list<int> yadayada(vals, vals + 10);
    // list<int> etcetera(vals, vals + 10);

    list<int> yadayada = {50, 100, 90, 150, 120, 70, 130, 110, 80, 140};
    list<int> etcetera = {50, 200, 290, 150, 220, 70, 130, 110, 80, 140};
    cout << "Original list:\n";

    for_each(yadayada.begin(), yadayada.end(), outint);
    cout << endl;
    for_each(etcetera.begin(), etcetera.end(), outint);
    cout << endl;
    yadayada.remove_if(f100);
    etcetera.remove_if(TooBig<int>(200));

    cout << "Trimmed lists:\n";

    for_each(yadayada.begin(), yadayada.end(), outint);
    cout << endl;

    for_each(etcetera.begin(), etcetera.end(), outint);

    cout << endl;
    return 0;
}