#include<iostream>
#include"studenti.h"

using std::cin;
using std::cout;
using std::endl;

void set(Student &sa,int n);
const int pupils=3;
const int quizzes=5;

int main()
{
    Student ada[pupils] = {Student(quizzes), Student(quizzes), Student(quizzes)};

    int i;
    for (i = 0; i < pupils; i++)
    {
        set(ada[i], quizzes);
    }
    cout << "\nStudent List:\n";
    for (i = 0; i < pupils; i++)
    {
        cout << ada[i].Name() << endl;
    }
    cout << "\nResults:";
    for (i = 0; i < pupils; i++)
    {
        cout << endl << ada[i];
        cout << "average:" << ada[i].Average() << endl;
    };
    cout << "\nDone.\n";
    return 0;
}

void set(Student &st, int n)
{
    cout << "Please enter the name of the student: ";
    getline(cin, st);
    cout << "Please enter " << n << " quizzes scores:\n";
    for (int i = 0; i < n; i++)
    {
        cin >> st[i];
    }
    while (cin.get() != '\n')
    {
        continue;
    }
}