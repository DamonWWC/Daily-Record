#include <iostream>
#include <string>
#include <set>
#include <algorithm>
#include <iterator>

int main()
{
    using namespace std;
    const int N = 6;
    string s1[N] = {"one", "two", "three", "four", "five", "one"};
    string s2[N] = {"metal", "any", "food", "elegant", "deliver", "for"};

    set<string> A(s1, s1 + N);
    set<string> B(s2, s2 + N);
    ostream_iterator<string> out(cout, " ");
    cout << "Set A: ";
    copy(A.begin(), A.end(), out);
    cout << endl;
    cout << "Set B: ";
    copy(B.begin(), B.end(), out);
    cout << endl;
    cout << "A union B: ";
    set_union(A.begin(), A.end(), B.begin(), B.end(), out);
    cout << endl;
    cout << "A intersection B: ";
    set_intersection(A.begin(), A.end(), B.begin(), B.end(), out);
    cout << endl;
    cout << "A-B: ";
    set_difference(A.begin(), A.end(), B.begin(), B.end(), out);
    cout << endl;

    set<string> C;
    cout << "Set C: ";
    set_union(A.begin(), A.end(), B.begin(), B.end(), insert_iterator<set<string>>(C, C.begin()));
    copy(C.begin(), C.end(), out);
    cout << endl;

    string s3("grungy");
    C.insert(s3);
    cout << "Set C after insertion: ";
    copy(C.begin(), C.end(), out);
    cout << endl;

    cout << "Showing a range \n";
    copy(C.lower_bound("any"), C.upper_bound("six"), out);
    cout << endl;
    return 0;
}