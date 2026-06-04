#include <iostream>
const double *f1(const double *, int);
const double *f2(const double *, int);
const double *f3(const double *, int);
int main()
{
    using namespace std;
    double av[3] = {10.1, 20.2, 30.3};
    const double *(*p1)(const double *, int) = f1;
    return 0;
}

const double *f1(const double *ar, int n)
{
    return ar; // ar=ar+0=&ar[0];
}

const double *f2(const double ar[], int n)
{
    return ar + 1;
}
const double *f3(const double ar[], int n)
{
    return ar + 3;
}