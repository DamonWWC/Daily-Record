#include <iostream>
using namespace std;

template<class T>
class ManyFriend
{
    private:
    T item;
    public:
    ManyFriend(const T &i):item(i){}
   template<class C,class D> friend void show2(C &, D &);

};

template<class C,class D> void show2(C &c,D &d)
{
    cout<<c.item<<", "<<d.item<<endl;
}

int main()
{
    ManyFriend<int>oi(10);
    ManyFriend<double>bo(10.5);
    ManyFriend<int>co(20);
    show2(oi,bo);
    return 0;
}