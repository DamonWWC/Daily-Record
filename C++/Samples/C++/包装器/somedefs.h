#include <iostream>
using namespace std;

template <class T, class F>
T use_f(T v, F f)
{
    static int count = 0;
    count++;
    cout << " use_f count = " << count << " , &count = " << &count << endl;
    return f(v);
}

class Fp
{
private:
    double z_;

public:
    Fp(double z = 1.0) : z_(z) {}
    double operator()(double p) { return p * z_; }
};
class Fq
{
private:
    double z_;

public:
    Fq(double z = 1.0) : z_(z) {}
    double operator()(double q) { return z_ + q; }
};