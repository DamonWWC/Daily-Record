#include <iostream>
#include <functional>
#include "somedefs.h"

double dub(double x) { return 2.0 * x; }
double square(double x) { return x * x; }
using namespace std;

int main()
{
    double y = 1.21;
    function<double(double)> ef1 = dub;
    function<double(double)> ef2 = square;
    function<double(double)> ef3 = Fq(10.0);
    function<double(double)> ef4 = Fp(10.0);
    function<double(double)> ef5 = [](double u)
    { return u * u; };
    function<double(double)> ef6 = [](double u)
    { return u + u / 2.0; };
    cout << "Function pointer dub:\n";
    cout << " " << use_f(y, ef1) << endl;
    cout << "Function pointer square:\n";
    cout << " " << use_f(y, ef2) << endl;
    cout << "Function object Fq:\n";
    cout << " " << use_f(y, ef3) << endl;
    cout << "Function object Fp:\n";
    cout << " " << use_f(y, ef4) << endl;
    cout << "Lambda expression:\n";
    cout << " " << use_f(y, ef5) << endl;
    cout << "Lambda expression:\n";
    cout << " " << use_f(y, ef6) << endl;

    return 0;
}