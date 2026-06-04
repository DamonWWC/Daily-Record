#pragma once

#include <iostream>
using std::istream;
using std::ostream;

class String
{
private:
    char *str;
    int len;
    static int num_strings;
    static const int CINLIM = 80;

public:
    // constructors and other methods
    String(const char *s);
    String();
    String(const String &st);
    ~String();
    int length() const { return len; }
    // overload operator methods
    String &operator=(const String &st);
    String &operator=(const char *s);
    char &operator[](int i);
    const char &operator[](int i) const;
    // friends
    friend bool operator<(const String &st1, const String &st2);
    friend bool operator>(const String &st1, const String &st2);
    friend bool operator==(const String &st1, const String &st2);
    friend ostream &operator<<(ostream &os, const String &st);
    friend istream &operator>>(istream &is, String &st);
    // static methods
    static int HowMany();
};