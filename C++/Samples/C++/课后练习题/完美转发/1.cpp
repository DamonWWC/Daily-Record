#include <iostream>
#include<utility>

using namespace std;

void process(int& i)
{
    cout<<"处理左值："<<i<<endl;
}

void process(int&& i)
{
    cout<<"处理右值："<<i<<endl;
}

template<typename T>
void logAndProcess(T&& t)
{
    process(forward<T>(t));
}

int main()
{
    int a=10;
    logAndProcess(a);
    logAndProcess(5);
    return 0;

}
// 移除重复元素不用额外空间
int removeDuplicates(int * nums,int numSize){
    if(numSize==0) return 0;
    int slow=0;
    for(int fast=1;fast<numSize;fast++){
        if(nums[fast]!=nums[slow])
        {
            nums[++slow]=nums[fast];
        }
    }
    return slow+1;
}