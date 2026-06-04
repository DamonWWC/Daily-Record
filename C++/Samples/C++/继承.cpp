#include <iostream>
#include <fstream>
int errors = 20;
// void file_it(ostream &os, double fo, const double *fe, int n);
// const int LIMIT = 5;
// using namespace std;
// int main()
// {
//     // ofstream fout;
//     // const char *fn = "test.txt";
//     // fout.open(fn);
//     // if (!fout.is_open())
//     // {
//     //     cout << "open file failed" << endl;
//     //     exit(EXIT_FAILURE);
//     // }
//     // double objective;
//     // cout << "Enter the number of the objective function: ";
//     // cin >> objective;
//     // double eps[LIMIT];
//     // for (int i = 0; i < LIMIT; i++)
//     // {
//     //     cout << "Enter the eps" << i + 1 << ": ";
//     //     cin >> eps[i];
//     // }
//     // file_it(cout, objective, eps, LIMIT);
//     // file_it(fout, objective, eps, LIMIT);

//     return 0;
// }
// void file_it(ostream &os, double fo, const double *fe, int n)
// {
//     os << "Focal length of objective: " << fo << "(mm)";
//     os << "f.l.eyepiece" << "       " << "magnification" << endl;
//     for (int i = 0; i < n; i++)
//     {
//         os << fe[i] << "mm" << "       " << int(fo / fe[i] + 0.5) << endl;
//     }
// }