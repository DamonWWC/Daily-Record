#include <iostream>
#include <string>
#include <iomanip>
#include <vector>
#include <fstream>
using namespace std;

class Person
{
public:
    friend ostream &operator<<(ostream &os, const Person &_person);
    friend istream &operator>>(istream &is, Person &_person);

public:
    string m_id;
    string m_name;
    string m_tel;
};
ostream &operator<<(ostream &os, const Person &_person)
{
    os << left << setw(15) << _person.m_id << left << setw(15) << _person.m_name << left << setw(15) << _person.m_tel << endl;
    return os;
}
istream &operator>>(istream &is, Person &_person)
{
    is >> _person.m_id >> _person.m_name >> _person.m_tel;
    return is;
}