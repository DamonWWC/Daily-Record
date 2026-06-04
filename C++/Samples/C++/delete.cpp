#include <iostream>
#include <cstring>
using namespace std;
char *getname(void);
int main(void)
{
    // char *name;
    // name = getname();
    // cout << name << " at " << (int *)name << endl;
    // delete[] name;
    // name = getname();
    // cout << name << " at " << (int *)name << endl;
    // delete[] name;

    char flower[10] = "rose";
    const char *bird = "rose";
    cout << "A concerned " << *flower << " speakse";
    return 0;
}
char *getname()
{
    char temp[80];
    cout << "Enter your name: ";
    cin >> temp;
    char *pn = new char[strlen(temp) + 1];
    strcpy(pn, temp);
    return pn;
}