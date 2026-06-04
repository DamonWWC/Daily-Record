#include <iostream>
#include <queue>
#include <vector>
#include <functional>

using namespace std;
int main()
{
    priority_queue<int, vector<int>, greater<int>> minHeap;

    minHeap.push(10);
    minHeap.push(30);
    minHeap.push(20);

    cout << "Min element: " << minHeap.top() << endl;

    minHeap.pop();

    cout << "Min element: " << minHeap.top() << endl;
    return 0;
}
