#include <iostream>
using std::cout;
using std::endl;

template <typename T>
class HasFriend
{
private:
    T item;
    static int ct;

public:
    HasFriend(const T &i) : item(i) { ct++; }
    ~HasFriend() { ct--; }
    friend void counts();
    friend void report(HasFriend<T> &);
};

template <typename T>
int HasFriend<T>::ct = 0;

void counts()
{
    cout << "int: " << HasFriend<int>::ct << "; ";
    cout << "double: " << HasFriend<double>::ct << endl;
}

void report(HasFriend<double> &hf)
{
    cout << "HasFriend<double>: " << hf.item << endl;
};

int main()
{
    cout<<"No objects declared:"<<endl;
    counts();
    HasFriend<int> hfi1(10);
    cout<<"After hfi1 declared:"<<endl;
    counts();
    HasFriend<int> hfi2(20);
    cout<<"After hfi2 declared:"<<endl;
    counts();
    HasFriend<double> hfdb(10.5);
    cout<<"After hfdb declared:"<<endl;
    counts();
    report(hfi1);
    report(hfi2);
    report(hfdb);
  
    return 0;

}