#include <iostream>
#include <string>
#include <iomanip>
#include <vector>
#include <fstream>
#include "Person.h"
using namespace std;

class PersonManager
{
public:
    void InputOnePerson(void);
    bool LoadAllPersonFromFile(const string &fileName);
    bool DeletePerson(void);
    bool QueryPersonByName() const;
    bool QueryPersonByTel() const;
    void ShowAllPerson(void) const;
    bool SaveAllPersonToFile(void) const;

private:
    vector<Person> m_allPerson;
};

bool PersonManager::DeletePerson(void)
{
    cout << "Please input person id for delete:";
    string id;
    cin >> id;
    for (auto itr = m_allPerson.cbegin(); itr != m_allPerson.cend(); ++itr)
    {
        if (itr->m_id == id)
        {
            m_allPerson.erase(itr);
            return true;
        }
    }
    return false;
}

bool PersonManager::QueryPersonByName() const
{
    cout << "Please input person name for query:";

    string name;
    cin >> name;
    for (auto itr = m_allPerson.cbegin(); itr != m_allPerson.cend(); ++itr)
    {
        if (itr->m_name == name)
        {
            cout << "Find:" << endl;
            cout << *itr << endl;
            return true;
        }
    }
    cout << "Not find!" << name << endl;
    return false;
}

bool PersonManager::QueryPersonByTel() const
{
    cout << "Please input person tel for query:";
    string tel;
    cin >> tel;
    for (auto itr = m_allPerson.cbegin(); itr != m_allPerson.cend(); ++itr)
    {
        if (itr->m_tel == tel)
        {
            cout << "Find:" << endl;
            cout << *itr << endl;
            return true;
        }
    }
    cout << "Not find!" << tel << endl;
    return false;
}

void PersonManager::ShowAllPerson(void) const
{
    cout << "All person:" << endl;
    cout << left << setw(15) << "id" << setw(15) << "name" << setw(15) << "tel" << endl;
    for (auto &item : m_allPerson)
    {
        cout << item << endl;
    }
}
bool PersonManager::SaveAllPersonToFile(void) const
{
    ofstream ofs("data_saved.txt");
    for (auto itr = m_allPerson.cbegin(); itr != m_allPerson.cend(); ++itr)
    {
        ofs << *itr << endl;
    }
    return true;
}

bool PersonManager::LoadAllPersonFromFile(const string &fileName)
{
    ifstream ifs(fileName);
    if (!ifs)
    {
        cout << "load data failed . file " << fileName << " not exits." << endl;
        return false;
    }
    Person person;
    while (ifs >> person)
    {
        m_allPerson.push_back(person);
    }
    cout << "load data from file success" << endl;
    return true;
}

void PersonManager::InputOnePerson(void)
{
    cout << "Please input person info:" << endl;
    cout << "Please input id:";
    string id;
    cin >> id;
    Person person;
    person.m_id = id;
    for (auto itr = m_allPerson.cbegin(); itr != m_allPerson.cend(); ++itr)
    {
        if (itr->m_id == id)
        {
            cout << id << " is exist!" << endl;
            return;
        }
    }
    cout << "Please input name:";
    string name;
    cin >> name;
    person.m_name = name;
    cout << "Please input tel:";
    string tel;
    cin >> tel;
    person.m_tel = tel;

    cout << "Input finished, save successed." << endl;
    m_allPerson.push_back(person);
}

int main()
{
    PersonManager personMgr;
    personMgr.LoadAllPersonFromFile("data.txt");
    personMgr.ShowAllPerson();
    while (true)
    {
        cout << "input a commond : " << endl;
        cout << "1 [AddPerson]" << endl;
        cout << "2 [ShowAllPerson]" << endl;
        cout << "3 [QueryPerson by name]" << endl;
        cout << "4 [QueryPerson by tel]" << endl;
        cout << "5 [SaveAllPersonToFile]" << endl;
        cout << "6 [DeletePerson]" << endl;
        cout << "0 [ExitAndSaveChange]" << endl;
        int commond;
        cin >> commond;
        switch (commond)
        {
        case 1:
        {
            personMgr.InputOnePerson();
            break;
        }
        case 2:
        {
            personMgr.ShowAllPerson();
            break;
        }
        case 3:
        {
            personMgr.QueryPersonByName();
            break;
        }
        case 4:
        {
            personMgr.QueryPersonByTel();
            break;
        }
        case 5:
        {
            personMgr.SaveAllPersonToFile();
            break;
        }
        case 6:
        {
            personMgr.DeletePerson();
            break;
        }
        case 0:
        {
            personMgr.SaveAllPersonToFile();
            return 0;
        }
        default:
        {
            cout << "System Exit." << endl;
            return 0;
        }
        }
    }
    return 0;
}