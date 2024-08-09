#include <iostream>
int main()
{
    char ch;
    int count = 0;
    std::cin.get(ch);
    while (std::cin.get(ch))
    {
        std::cout << ch;
        ++count;
        std::cin.get(ch);
    }
    std::cout << std::endl
              << count << " characters read\n";
    return 0;
}