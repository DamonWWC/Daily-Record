#include <iostream>
#include <string>

using namespace std;

class Cpmv
{
public:
    struct Info
    {
        string qcode;
        string zcode;
    };

private:
    Info *pi;

public:
    Cpmv();
    Cpmv(string q, string z);
    Cpmv(const Cpmv &cp);
    Cpmv(Cpmv &&mv);
    ~Cpmv();
    Cpmv &operator=(const Cpmv &cp);
    Cpmv &operator=(Cpmv &&mv);
    Cpmv operator+(const Cpmv &obj) const;
    void Display() const;
};

Cpmv::Cpmv()
{
    pi = nullptr;
}

Cpmv::Cpmv(string q, string z)
{
    pi = new Info;
    pi->qcode = q;
    pi->zcode = z;
    cout << "Constructed with args" << endl;
}
// 复制构造函数
Cpmv::Cpmv(const Cpmv &cp)
{
    pi = new Info;
    pi->qcode = cp.pi->qcode;
    pi->zcode = cp.pi->zcode;
    cout << "Constructed with copy" << endl;
}
// 移动构造函数
Cpmv::Cpmv(Cpmv &&mv)
{
    pi = mv.pi;
    mv.pi = nullptr;
    cout << "Constructed with move" << endl;
}
Cpmv::~Cpmv()
{
    delete pi;
    cout << "Destructed" << endl;
}
// 赋值操作符重载
Cpmv &Cpmv::operator=(const Cpmv &cp)
{
    if (this == &cp)
    {
        return *this;
    }
    delete pi;
    pi = new Info;
    pi->qcode = cp.pi->qcode;
    pi->zcode = cp.pi->zcode;
    cout << "Assigned with copy" << endl;
    return *this;
}
// 移动赋值操作符重载
Cpmv &Cpmv::operator=(Cpmv &&mv)
{
    if (this == &mv)
    {
        return *this;
    }
    delete pi;
    pi = mv.pi;
    mv.pi = nullptr;
    cout << "Assigned with move" << endl;
    return *this;
}
// 加法操作符重载
Cpmv Cpmv::operator+(const Cpmv &obj) const{
    Cpmv temp;
    temp.pi = new Info;
    temp.pi->qcode = pi->qcode + obj.pi->qcode;
    temp.pi->zcode = pi->zcode + obj.pi->zcode;
    cout<<"Operator +"<<endl;
    return temp;
}
// 显示函数
void Cpmv::Display() const
{
    cout<<"Display Info"<<endl;
    if(pi==nullptr)
    {
        cout<<"pi is nullptr"<<endl;
    }

    cout << "qcode: " << pi->qcode << endl;
    cout << "zcode: " << pi->zcode << endl;
}

int main()
{
}