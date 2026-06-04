#include <iostream>
#include <string>
using namespace std;
void show_list3() {}

template <class T>
void show_list3(const T &value)
{
    cout << value << "\n";
}

template <class T, class... Args>
void show_list3(const T &value, const Args &...args)
{
    cout << value << ", ";
    show_list3(args...);
}

int main()
{
    int n = 14;
    double x = 2.71828;
    string mr = "Mr. String";
    show_list3(n, x);
    show_list3(x * n, "hello", mr);
    return 0;
}