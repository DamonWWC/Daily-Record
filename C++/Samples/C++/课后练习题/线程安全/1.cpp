#include <iostream>
#include <thread>
#include <memory>
#include <mutex>
#include <vector>
#include <future>
using namespace std;

shared_ptr<int> shared_data = make_shared<int>(0);
mutex mtx;

void increment_data()
{
    lock_guard<mutex> lock(mtx);
    (*shared_data)++;
    cout << "Data incremented by thread " << *shared_data << endl;
}

void increment_data_async(promise<void> done)
{
    (*shared_data)++;
    cout << "Data incremented by thread " << *shared_data << endl;
    done.set_value();
}

int main()
{
    // vector<thread>threads;
    // for(int i=0;i<10;i++)
    // {
    //     threads.emplace_back(increment_data);
    // }
    // for(auto& t:threads){
    //     t.join();
    // }
    // cout<<"Final data value:"<<*shared_data<<endl;

    vector<future<void>> futures;
    for (int i = 0; i < 10; i++)
    {
        promise<void> promise;
        futures.push_back(promise.get_future());
        thread(increment_data_async, move(promise)).detach();
    }
    for (auto &f : futures)
    {
        f.get();
    }
    cout << "Final data value:" << *shared_data << endl;

    return 0;
}