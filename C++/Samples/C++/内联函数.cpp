#include <iostream>
inline double square(double x);
int main()
{
    using namespace std;
    double a,b;
    a=square(5.0);
    cout<<a<<endl;
    return 0;
    
}

inline double square(double x)
{
 return x*x;
}