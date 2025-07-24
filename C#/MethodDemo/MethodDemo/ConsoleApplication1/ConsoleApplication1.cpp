// ConsoleApplication1.cpp : 此文件包含 "main" 函数。程序执行将在此处开始并结束。
//

#include <iostream>
#define  BUILD  WINDOWS #include <Windows.h>
#include<windows.h>

#include <memory>
#include<string>
#include<set>

using namespace std;

int wmain(int argc, wchar_t* argv[]) {
    auto size = 1 << 14;
    unique_ptr<WCHAR[]> buffer;
    for (; ;) {
        buffer = make_unique<WCHAR[]>(size);
        if (0 == ::QueryDosDevice(nullptr, buffer.get(), size)) {
            if (::GetLastError() == ERROR_INSUFFICIENT_BUFFER) {
                size *= 2;
                continue;
            }
            else {
                printf("QueryDosDevice failed with error %d\n", ::GetLastError());
                return 1;
            }
        }
        else
            break;
    }
    if (argc > 1) {
        // convert argument  to  lowercase
        ::_wcslwr_s(argv[1], ::wcslen(argv[1]) + 1);
    }

    auto filter = argc > 1 ? argv[1] : nullptr;
    // simplify stored  type
    using  LinkPair = pair<wstring, wstring>;
    struct LessNoCase {
        bool  operator()(const  LinkPair& p1, const  LinkPair& p2) const {
            return ::_wcsicmp(p1.first.c_str(), p2.first.c_str()) < 0;
        }
    };

    // sorted by LessNoCase
    set<LinkPair, LessNoCase> links;
    WCHAR target[512];

    for (auto p = buffer.get(); *p; ) {
        wstring name(p);
        auto locase(name);
        ::_wcslwr_s((wchar_t*)locase.data(), locase.size() + 1);
        if (filter == nullptr || locase.find(filter) != wstring::npos) {
            ::QueryDosDevice(name.c_str(), target, _countof(target));
            // add pair  to results
            links.insert({ name, target });
        }

        // move  to next  item
        p += name.size() + 1;
    }

    // print results
    for (auto& link : links) {
        printf("%ws = %ws\n", link.first.c_str(), link.second.c_str());
    }

}

//int main(int argc,CONST char* argv[],CHAR* env[])
//{
//    SYSTEM_INFO si;
//    ::GetNativeSystemInfo(&si);
//    printf("Number of Logical Processors: %d\n", si.dwNumberOfProcessors);
//    printf("Page size: %d Bytes\n", si.dwPageSize);
//    printf("Processor Mask: 0x%p\n", (PVOID)si.dwActiveProcessorMask);
//    printf("Minimum process address: 0x%p\n", si.lpMinimumApplicationAddress);
//    printf("Maximum process address: 0x%p\n", si.lpMaximumApplicationAddress);
//    std::cout << "Hello World!\n";
//
//    for (int i = 0; ; i++) {
//        if (env[i] == nullptr)
//            break;
//
//        auto equals = strchr(env[i], '=');
//        // 将等号替换为NULL
//        *equals = '\0';
//        printf("%s: %s\n", env[i], equals + 1);
//        // 为保持一致性，恢复等号
//        *equals = '=';
//    }
//
//    //OSVERSIONINFO vi = { sizeof(vi) };
//    //::GetVersionEx(&vi);
//    //printf("OS Version: %d.%d\n", vi.dwMajorVersion, vi.dwMinorVersion);
//    return 0;
//}

// 运行程序: Ctrl + F5 或调试 >“开始执行(不调试)”菜单
// 调试程序: F5 或调试 >“开始调试”菜单

// 入门使用技巧: 
//   1. 使用解决方案资源管理器窗口添加/管理文件
//   2. 使用团队资源管理器窗口连接到源代码管理
//   3. 使用输出窗口查看生成输出和其他消息
//   4. 使用错误列表窗口查看错误
//   5. 转到“项目”>“添加新项”以创建新的代码文件，或转到“项目”>“添加现有项”以将现有代码文件添加到项目
//   6. 将来，若要再次打开此项目，请转到“文件”>“打开”>“项目”并选择 .sln 文件
