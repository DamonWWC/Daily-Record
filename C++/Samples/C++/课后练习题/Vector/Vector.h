#include<iostream>

using namespace std;

template<typename T>
class Vector
{
    friend ostream &operator<< <T>(ostream &out,const Vector &object);
    public:
    Vector(int size=128);
    Vector(const Vector &object);//�������캯��

    int getLength() const;

    T& operator[](int index);

    Vector &operator=(const Vector &object);

    ~Vector();

    private:
    T *m_base;
    int m_len;
};
