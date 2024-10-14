#pragma once
#include <QWidget>
#include<QDebug>
class teacher:public QWidget
{
	Q_OBJECT
public:
	void treat()
	{
		qDebug()<<"teacher treat";
	}
	void treat(QString name)
	{
		qDebug() << "teacher treat "<<name;
	}
public:
	teacher(QWidget* parent = nullptr);
};

