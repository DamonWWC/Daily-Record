#include <iostream>
double cube(double a);
double recube(const double& ra);
int main()
{
    using namespace std;
    int rats = 101;
    int &rodents = rats;
    cout << rats << endl;
    cout << rodents << endl;
    rodents++;
    cout << rats << endl;
    cout << rodents << endl;

    double x = 3.0;
    cout << cube(x);
    cout << "cube of" << x << endl;
    cout << recube(x);
    cout << "cube of" << x << endl;
    return 0;
}

double cube(double a)
{
    a *= a * a;
    return a;
}
double recube(const double& ra)
{
   
    return ra*ra*ra;
}