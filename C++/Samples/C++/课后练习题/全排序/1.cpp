#include <iostream>
#include <vector>

using namespace std;

void printArrangement(const vector<int> &arr)
{
    for (int num : arr)
    {
        cout << num << " ";
    }
    cout << endl;
}

void generateAppPermutations(vector<int> &arr, int index)
{
    if (index >= arr.size())
    {
        printArrangement(arr);
        return;
    }
    for (int i = index; i < arr.size(); i++)
    {
        swap(arr[index], arr[i]);
        generateAppPermutations(arr, index + 1);
        swap(arr[index], arr[i]);
    }
}
int main()
{
    vector<int> arr = {4,1, 2, 3};
    generateAppPermutations(arr, 0);
    return 0;
}