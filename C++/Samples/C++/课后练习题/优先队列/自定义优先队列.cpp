#include <iostream>
#include <queue>
#include <vector>
using namespace std;
struct Compare
{
    bool operator()(const pair<int, string> &a, const std::pair<int, string> &b)
    {
        return a.second > b.second;
    }
};

int main()
{
    priority_queue<pair<int, string>, vector<pair<int, string>>, Compare> customPQ;
    customPQ.push({10, "Alice"});
    customPQ.push({30, "Bob"});
    customPQ.push({20, "Charlie"});

    cout << "Top element: " << customPQ.top().second << "with priority: " << customPQ.top().first << endl;
    customPQ.pop();

    cout << "Top element: " << customPQ.top().second << "with priority: " << customPQ.top().first << endl;
    return 0;
}