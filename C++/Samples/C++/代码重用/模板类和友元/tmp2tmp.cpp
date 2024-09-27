#include<iostream>

using namespace std;

template<class T>void counts();
template<class T>void report(T &);

template<class TT>
class HasFriendT
{
    private:
    TT item;
    static int ct;
    public:
    HasFriendT(const TT & i):item(i){ct++;}
    ~HasFriendT(){ct--;}
    friend void counts<TT>();
    friend void report<>(HasFriendT<TT> &);

};

template<class T>
int HasFriendT<T>::ct = 0;

template<class T>
void counts()
{
    cout<<"template size: "<<sizeof(HasFriendT<T>)<<"; ";
    cout<<"counts: "<<HasFriendT<T>::ct<<endl;
}

template<class T>
void report(T & hf)
{
    cout<<hf.item<<endl;
}

int main()
{
    counts<int>();
    HasFriendT<int> hfi1(10);
    HasFriendT<int> hfi2(20);
    report(hfi1);
    report(hfi2);
    counts<int>();
    HasFriendT<double> hfdb(10.5);
    report(hfdb);
    counts<double>();
    return 0;
}