#include <iostream>

using namespace std;

long double sum_value() { return 0; };

template <class T, class... Args>
long double sum_value(T value, const Args &...args)
{
    (long double)sum = (long double)value += sum_value(args...);
    return sum;
}

int main(void)
{
}