#include <iostream>
using namespace std;
#include "Vector.h"

template <typename T>
ostream &operator<<(ostream &out, const Vector<T> &object)
{
    for (int i = 0; i < object.m_len; i++)
    {
        out << object.m_base[i] << " ";
    }
    cot << endl;

    return out;
}

template <typename T>
Vector<T>::Vector(const Vector<T> &object)
{
    delete[] m_base;

    m_len = object.m_len;
    m_base = new T[m_len];
    for (int i = 0; i < m_len; i++)
    {
        m_base[i] = object.m_base[i];
    }
}

template <typename T>
int Vector<T>::getLength() const
{
    return m_len;
}

template <typename T>
T &Vector<T>::operator[](int index)
{
    return m_base[index];
}

template <typename T>
Vector<T> &Vector<T>::operator=(const Vector<T> &object)
{
    if (m_base != NULL)
    {
        delete[] m_base;
        m_base = NULL;
        m_len = 0;
    }

    m_len = object.m_len;
    m_base = new T[m_len];

    for (int i = 0; i < m_len; i++)
    {
        m_base[i] = object.m_base[i];
    }
    return *this;
}

