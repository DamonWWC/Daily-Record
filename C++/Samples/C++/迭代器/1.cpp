#include <iostream>
#include <list>

using namespace std;

int main()
{
    



    list<int> l;
    l.push_back(1);
    l.push_back(2);
    l.push_back(3);

    list<int>::iterator it;
   
    for (it = l.begin(); it != l.end(); it++)
    {
        //*it += 1;
        cout << *it << " ";
    }
    cout << endl;

    cout << l.front() << endl;
    cout << l.back() << endl;
    l.pop_front(); // ´Ó¶¥²¿É¾³ıÔªËØ

    list<int>::const_iterator it1;
    for (it1 = l.begin(); it1 != l.end(); it1++)
    {
        //*it += 1;
        cout << *it1 << " ";
    }
}