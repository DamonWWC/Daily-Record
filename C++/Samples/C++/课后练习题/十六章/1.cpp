#include <iostream>
#include <string>
#include <algorithm>
#include <cctype>

using namespace std;

int main()
{
    string str;
    cout << "enter a string:";
    getline(cin, str);
    if (same_after_reverse(str))
    {
        cout << "yes" << endl;
    }
    else
    {
        cout << "no" << endl;
    }
}

bool same_after_reverse(const string &str)
{
    string temp;
    for (int i = 0; i < str.size(); i++)
    {
        if (isalpha(str[i]))
            temp += tolower(str[i]);
    }
    string temp1 = temp;
    reverse(temp1.begin(), temp1.end());
    return temp == temp1;
}