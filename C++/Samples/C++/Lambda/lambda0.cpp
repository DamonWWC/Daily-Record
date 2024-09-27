#include <iostream>
#include <vector>
#include <algorithm>
#include <cmath>
#include <ctime>

const long Size1 = 39L;
const long Size2 = 100 * Size1;
const long Size3 = 100 * Size2;
bool fs(int x) { return x % 3 == 0; }
bool f13(int x) { return x % 13 == 0; }

using namespace std;

int main()
{
    vector<int> numbers(Size1);
    srand(time(0));
    generate(numbers.begin(), numbers.end(), rand);
    // using function pointers

    cout << "Sample size=" << Size1 << endl;
    int count3 = count_if(numbers.begin(), numbers.end(), fs);
    cout << "Count of numbers divisible by 3:" << count3 << endl;
    int count13 = count_if(numbers.begin(), numbers.end(), f13);
    cout << "Count of numbers divisible by 13:" << count13 << endl;

    // increase number of numbers

    numbers.resize(Size2);
    generate(numbers.begin(), numbers.end(), rand);
    cout << "Sample size=" << Size2 << endl;
    // using s functor
    class f_mod
    {
    private:
        int dv;

    public:
        f_mod(int d = 1) : dv(d) {}
        bool operator()(int x) { return x % dv == 0; }
    };
    count3 = count_if(numbers.begin(), numbers.end(), f_mod(3));
    cout << "Count of numbers divisible by 3:" << count3 << endl;
    count13 = count_if(numbers.begin(), numbers.end(), f_mod(13));
    cout << "Count of numbers divisible by 13:" << count13 << endl;

    // increase number of numbers
    numbers.resize(Size3);
    generate(numbers.begin(), numbers.end(), rand);
    cout << "Sample size=" << Size3 << endl;
    // using lambdas
    count3 = count_if(numbers.begin(), numbers.end(), [](int x)
                      { return x % 3 == 0; });
    cout << "Count of numbers divisible by 3:" << count3 << endl;
    count13 = count_if(numbers.begin(), numbers.end(), [](int x)
                       { return x % 13 == 0; });
    cout << "Count of numbers divisible by 13:" << count13 << endl;

    count13 = 0;
    for_each(numbers.begin(), numbers.end(), [&count13](int x)
             { count13 += x % 13 == 0; });
    cout << "Count of numbers divisible by 13:" << count13 << endl;

    count3 = count13 = 0;
    for_each(numbers.begin(), numbers.end(), [&](int x)
             {count3+=x%3==0; count13+=x%13==0; });
    cout << "Count of numbers divisible by 3:" << count3 << endl;
    cout << "Count of numbers divisible by 13:" << count13 << endl;

    return 0;
};
