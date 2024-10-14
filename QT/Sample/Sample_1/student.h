#pragma once
#include <QWidget>
class student:public QWidget
{
	Q_OBJECT
		signals:
	void hungry();
	void hungry(QString name);
public:
	void increment(QString name)
	{
		emit hungry(name);
	}
public:
	student(QWidget* parent = nullptr);
	
};

