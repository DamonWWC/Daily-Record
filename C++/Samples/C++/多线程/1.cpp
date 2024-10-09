#include <iostream>
#include <thread>

using namespace std;

void printMessage(int count)
{
    for (int i = 0; i < count; i++)
    {
        cout << "Hello from thread (function pointer)!\n";
    }
}
/// ·Âº¯Êý
struct PrintTask
{
    void operator()(int count) const
    {
        for (int i = 0; i < count; i++)
        {
            cout << "Hello from thread (function object)!\n";
        }
    }
};

void increment(int &i)
{
    i++;
}
int main()
{
   
    thread t1(PrintTask(), 10);
    t1.join();



     thread t3([](int count){
        for (int i = 0; i < count; i++)
        {
            cout << "Hello from thread (lambda)!\n";
        }
    },5);
    t3.join();


    int num=0;
    thread t(increment, ref(num));
    t.join();
    cout << "Value after increment: " << num << endl;
    return 0;
}
