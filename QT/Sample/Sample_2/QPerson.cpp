#include "QPerson.h"
#include<QObject>

QPerson::QPerson(QString fName,QObject* parent) :QObject(parent)
{
	m_name = fName;
}

unsigned QPerson::age()const
{
	return m_age;
}
void QPerson::setAge(unsigned age)
{
	m_age = age;
	emit ageChanged(age);
}

void QPerson::incAge()
{
	m_age++;
	emit ageChanged(m_age);
}