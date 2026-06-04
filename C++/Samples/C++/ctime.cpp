#include<iostream>
#include<ctime>
int main()
{
    using namespace std;
    float secs;
    cin>>secs;
    clock_t delay=secs*CLOCKS_PER_SEC;
    cout<<"sleeping for "<<secs<<" seconds"<<endl;
    clock_t start=clock();
    while(clock()-start<delay)
    {
        
    }
    
    cout<<"done\a\n";
    return 0;
}