#include <iostream>
#include <cstring>
using namespace std;

#include "Vector.cpp"

class Student
{
    friend ostream &operator<<(ostream &out, const Student &object);

public:
    Student()
    {
        age = 0;
        name[0] = '\0';
    }
    Student(int _age, char *_name)
    {
        age = _age;
        strcpy_s(name, _name);
    }
    void print()
    {
        cout << name << ", " << age << endl;
    }
    ~Student() {}

private:
    int age;
    char name[64];
};

ostream &operator<<(ostream &out, const Student &object)
{
    out << object.name << ", " << object.age << endl;
    return out;
}

int main()
{
    Student s1(18, "zhangsan");
    Student s2(19, "lisi");

    Vector<Student *>studentVector(2);
    
    studentVector[0] = &s1;
    studentVector[1] = &s2;

    cout<<studentVector<<endl;
    system("pause");

    


}