#include<iostream>
#include<string>
#include<memory>

class Report
{
    private:
    std::string str;
    public:
    Report(const std::string s) :str(s)
    {
        std::cout<<"Object created\n";
    }
    ~Report(){std::cout<<"Object deleted:\n";}
    void comment() const{std::cout<<str<<"\n";}

};

int main()
{
    // {
    //     std::auto_ptr<Report>ap(new Report("using auto_ptr"));
    //     ap->comment();
    // }
    {
        std::shared_ptr<Report>ps(new Report("using shared_ptr"));
        ps->comment();
    }
    {
        std::unique_ptr<Report>pu(new Report("using unique_ptr"));
        pu->comment();
    }
}