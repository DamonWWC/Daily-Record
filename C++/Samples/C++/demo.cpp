#include <climits>
#include <iostream>
#include <cmath>
#include <string>
#include <cstring>
#define INT_MAX2 32767

struct inflatable
{
    std::string name;
    float volume;
    double weight;
} mr_smith = {
    "Mr. Smith",
    1.88,
    21.24};
int main()
{
    using namespace std;
    // int carrots;
    // cout << "How many carrots do you have?" << INT_MAX2 << endl;
    // // cin >> carrots;
    // cout << "Here are two more. ";
    // carrots = carrots + 2;
    // cout << "Now you have " << carrots << " carrots." << endl;
    // int hamburgers = {24};
    // int emus{2};
    // int rocs = {};
    // cout << "hamburgers=" << hamburgers << endl;

    // short sam = SHRT_MAX;
    // unsigned short sue = sam;
    // sam = sam + 1;
    // sue = sue + 1;
    // cout << "sam has " << sam << " dollars and sue has " << sue << " dollars." << endl;

    // int chest = 42;
    // cout << oct;
    // cout << "chest=" << chest << endl;

    // int cards[4] = {1, 2, 3, 4};

    inflatable guest =
        {
            "Glorious Gloria",
            1.88,
            12.99};
    cout << "Guest=" << mr_smith.name << endl;

    int donuts = 6;
    cout << "donuts=" << donuts << "and donuts address =" << &donuts << endl;

    int updates = 6;
    int *p_updates;
    p_updates = &updates;
    cout << "updates=" << updates << endl;
    cout << "*p_updates=" << *p_updates << endl;
    int *pn = new int;

    int *psome = new int[10];

    delete[] psome;

    inflatable *ps = new inflatable;
    cout << "Enter name of inflatable item: ";
    //cin.getline(ps->name, 20);
    delete ps;
    return 0;
}
