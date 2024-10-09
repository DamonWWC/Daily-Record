#include <iostream>
#include <thread>
using namespace std;

void foo(int Z)
{
    for (int i = 0; i < Z; i++)
    {
        cout << "�߳�ʹ�ú���ָ����Ϊ�ɵ��ò���\n";
    }
}

class ThreadObj
{
public:
    void operator()(int x) const
    {
        for (int i = 0; i < x; i++)
        {
            cout << "�߳�ʹ�ú���������Ϊ�ɵ��ò���\n";
        }
    }
};

int main()
{
    cout << "�߳�1��2��3��������" << endl;
    // ʹ�ú���ָ�봴���߳�
    thread th1(foo, 3);

    // ʹ�ú������󴴽��߳�
    thread th2(ThreadObj(), 3);

    // ʹ��lambda���ʽ�����߳�
    thread th3([](int x)
               {
        for(int i=0;i<x;i++)
        {
            cout<<"�߳�ʹ��lambda���ʽ��Ϊ�ɵ��ò���\n";
        } }, 3);

    th1.join();
    th2.join();
    th3.join();
    return 0;

}