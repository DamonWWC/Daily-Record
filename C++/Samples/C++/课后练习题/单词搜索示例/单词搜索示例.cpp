#include <iostream>
#include <iomanip>
#include <fstream>
#include <list>
#include <map>
#include <memory>
#include <set>
#include <string>
#include <sstream>
#include <vector>
using namespace std;

class CQueryResult
{
public:
    CQueryResult() {}
    CQueryResult(const string &queryWord, shared_ptr<set<size_t>> lines, shared_ptr<vector<string>> lineText) : m_queryWord(queryWord), m_lines(lines), m_lineText(lineText) {};
    friend ostream &operator<<(ostream &os, const CQueryResult &queryResult);

private:
    string m_queryWord;
    shared_ptr<set<size_t>> m_lines;
    shared_ptr<vector<string>> m_lineText;
};
ostream &operator<<(ostream &os, const CQueryResult &queryResult)
{
    os << queryResult.m_queryWord << " occurs " << queryResult.m_lines->size() << " times " << endl;
    for (auto line : *queryResult.m_lines)
    {
        os << line << ":"<< queryResult.m_lineText->at(line) << endl;
    }
    os << endl;
    return os;
}

class CTextQuery
{
public:
    CTextQuery(ifstream &_fin);
    CQueryResult query(const string &_queryWord) const;

public:
    friend ostream &operator<<(ostream &_os, const CTextQuery &_textQuery);

private:
    shared_ptr<vector<string>> m_allLineText;
    map<string, shared_ptr<set<size_t>>> m_wordMap;
};

CTextQuery::CTextQuery(ifstream &_fin) : m_allLineText(new vector<string>)
{
    string lineText;
    int currentLine = 0;
    while (getline(_fin, lineText))
    {
        m_allLineText->push_back(lineText);
        istringstream iss(lineText);
        string word;
        while (iss >> word)
        {
            auto &lines = m_wordMap[word];
            if (!lines)
            {
                lines.reset(new set<size_t>);
            }
            lines->insert(currentLine);
        }
        ++currentLine;
    }
}

CQueryResult CTextQuery::query(const string &_queryWord) const
{
    auto lines = m_wordMap.find(_queryWord)->second;
    return CQueryResult(_queryWord, lines, m_allLineText);
}

int main()
{
    string file("input_file.txt");
    ifstream fin(file);
    if (!fin)
    {
        cerr << "file open failed!" << endl;
        return -1;
    }
    CTextQuery textQuery(fin);
    CQueryResult queryResult = textQuery.query("second");
    cout << queryResult << endl;
    return 0;
}
