#include <memory>
#include<string>
#include<set>

using namespace std;

int wmain(int argc,wchar_t* argv[]){
    auto size=1<<14;
    unique_ptr<WCHAR[]> buffer;
    for(; ;){
        buffer=make_unique<WCHAR[]>(size);
        if(0 == ::QueryDosDevice(nullptr,buffer.get(),size)){
            if(::GetLastError() == ERROR_INSUFFICIENT_BUFFER){
                size *=2;
                continue;
            }
            else{
                printf("QueryDosDevice failed with error %d\n",::GetLastError());
                return 1;
            }
        }
        else 
            break;
    }
}