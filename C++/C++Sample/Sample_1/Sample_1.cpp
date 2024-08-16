// Sample_1.cpp : 此文件包含 "main" 函数。程序执行将在此处开始并结束。
//

#include <iostream>
const int ArSize = 10;
void strcout(const char* str);
int main()
{
	using namespace std;
	char input[ArSize];
	char next;
	cout<<"Enter a line:\n";
	cin.get(input, ArSize);
	while (cin)
	{
		cin.get(next);
		while (next != '\n')
		{
			cin.get(next);
		}
		strcout(input);
		cout << "Enter next line (empty line to quit):\n";
		cin.get(input, ArSize);
	}

		return 0;
}

void strcout(const char* str)
{
	using namespace std;
	int count = 0;
	static	int total = 0;
	cout << "\"" << str << "\" contains\n";
	while (*str++)
	{
		count++;
	}
	total += count;
	cout << count << endl;
	cout << total << endl;
}