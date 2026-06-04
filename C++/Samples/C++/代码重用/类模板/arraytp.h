#pragma once

#include <iostream>
#include <cstdlib>

template <class T, int n>
class ArrayTP
{
private:
    T arr[n];

public:
    ArrayTP() {};
    explicit ArrayTP(const T &v);
    virtual T &operator[](int i);      // 可修改数组里的内容
    virtual T operator[](int i) const; // 只能读取数组的内容
};

template <class T, int n>
ArrayTP<T, n>::ArrayTP(const T &v)
{
    for (int i = 0; i < n; i++)
    {
        arr[i] = v;
    }
}

template <class T, int n>
T &ArrayTP<T, n>::operator[](int i)
{
    if (i < 0 || i >= n)
    {
        std::cerr << "Error in array limits: " << i << " is out of range\n";
        std::exit(EXIT_FAILURE);
    }
    return ar[i];
}

template <class T, int n>
T ArrayTP<T, n>::operator[](int i) const
{
    if (i < 0 || i >= n)
    {
        std::cerr << "Error in array limits: " << i << " is out of range\n";
        std::exit(EXIT_FAILURE);
    }
    return ar[i];
}