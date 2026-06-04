#include <iostream>
#include <cctype>
#include <fstream>
#include <cstdlib>

int main()
{
    using namespace std;
    std::cout << "Enter text for analysis, and type @ to terminate input.\n";
    char ch;
    int whitespace = 0;
    int digits = 0;
    int chars = 0;
    int punct = 0;
    int others = 0;

    // cin.get(ch);
    // while (ch != '@')
    // {
    //     if (isalpha(ch))
    //         chars++;
    //     else if (isspace(ch))
    //         whitespace++;
    //     else if (isdigit(ch))
    //         digits++;
    //     else if (ispunct(ch))
    //         punct++;
    //     else
    //         others++;
    //     cin.get(ch);
    // }

    // ofstream outFile;
    // outFile.open("analysis.txt");
    // outFile << fixed;
    // outFile.precision(2);
    // outFile << "Make and model" << endl;

    char filename[80];

    ifstream inFile;
    cout << "Enter name of data file: ";
    cin.getline(filename, 80);
    inFile.open(filename);
    if (!inFile.is_open())
    {
        cout << "Could not open the file " << filename << endl;
        cout << "Program terminating.\n";
        exit(EXIT_FAILURE);
    }

    double value;
    double sum = 0.0;
    int count = 0;
    while (inFile >> value)
    {
        ++count;
        sum += value;
    }
    if (inFile.eof())
        cout << "End of file reached.\n";
    else if (inFile.fail())
        cout << "Input terminated by data mismatch.\n";
    else
        cout << "Input terminated by unknown reason.\n";
    if (count == 0)
        cout << "No data processed.\n";
    else
    {
        cout<<"Items read:"<<count<<endl;
        cout<<"Sum of the items:"<<sum<<endl;
        cout<<"Average of the numbers is "<<sum/count<<endl;

    }
    inFile.close();

    return 0;
}