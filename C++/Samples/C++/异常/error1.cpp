#include <iostream>
#include <cstdlib>

double hmean(double a, double b);

int main()
{

    double x,y,z;
    std::cout<<"Enter two numbers: ";
    while(std::cin>>x>>y)
    {
        z=hmean(x,y);
        std::cout<<"Harmonic mean of "<<x<<" and "<<y<<" is "<<z<<std::endl;
        std::cout<<"Enter next set of numbers <q to quit>: ";
    }
    std::cout<<"Bye!"<<std::endl;
    return 0;
}

double hmean(double a, double b)
{
    if(a==-b)
    {
        std::cout<<"Invalid arguments: a = -b not allowed."<<std::endl;
        std::abort();
    }
    return 2.0*a*b/(a+b);

}