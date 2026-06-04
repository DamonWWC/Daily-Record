#include <iostream>
#include <thread>
using namespace std;

void foo(int Z)
{
    for (int i = 0; i < Z; i++)
    {
        cout << "线程使用函数指针作为可调用参数\n";
    }
}

class ThreadObj
{
public:
    void operator()(int x) const
    {
        for (int i = 0; i < x; i++)
        {
            cout << "线程使用函数对象作为可调用参数\n";
        }
    }
};

int main()
{
    cout << "线程1、2、3独立运行" << endl;
    // 使用函数指针创建线程
    thread th1(foo, 3);

    // 使用函数对象创建线程
    thread th2(ThreadObj(), 3);

    // 使用lambda表达式创建线程
    thread th3([](int x)
               {
        for(int i=0;i<x;i++)
        {
            cout<<"线程使用lambda表达式作为可调用参数\n";
        } }, 3);

    th1.join();
    th2.join();
    th3.join();
    return 0;

}