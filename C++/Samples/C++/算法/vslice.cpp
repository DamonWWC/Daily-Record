#include <iostream>
#include <valarray>
#include <cstdlib>

const int SIZE = 12;
typedef std::valarray<int> vint;
void show(const vint &v, int cols);

int main()
{
    using namespace std;
    vint valint(SIZE);
}

void show(const vint &v, int cols)
{
    using namespace std;
    int lim = v.size();
    for (int i = 0; i < lim; i++)
    {
        cout.width(3);
        cout << v[i];
        if (i % cols == cols - 1)
        {
            cout << endl;
        }
        else
        {
            cout << ' ';
        }
    }
    if (lim % cols != 0)
    {
        cout << endl;
    }
}