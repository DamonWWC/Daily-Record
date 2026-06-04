#include <iostream>
#include<initializer_list>
using namespace std;

template<class T>
T average_list(initializer_list<T> i)
{
    T sum=0;
    int count=0;
    for(auto p= i.begin();p<=i.end(),p++)
    {
        count++;
        sum+=*p;
    }
    return sum/count;
}
int main()
{
auto q=average_list({15.4,10.7,9.0});
cout<<q<<endl;

cout<<average_list({20,30,19,17,45,38})<<endl;

auto ad=average_list<double>({'A',70,65.33});
cout<<ad<<endl;
return 0;


}