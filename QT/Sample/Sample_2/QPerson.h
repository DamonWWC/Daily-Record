#pragma once
#include <QObject>
class QPerson : public QObject
{
	Q_OBJECT
	Q_CLASSINFO("Author", "wang")
	Q_CLASSINFO("Version", "1.0")
	Q_CLASSINFO("Description", "Person")
	Q_PROPERTY(unsigned age READ age WRITE setAge NOTIFY ageChanged)
	Q_PROPERTY(QString name MEMBER m_name)
	Q_PROPERTY(int score MEMBER m_score)
public:
private:
	unsigned m_age = 10;
	QString m_name;
	int m_score = 79;
public:
	explicit  QPerson(QString fName,QObject* parent = nullptr);
	unsigned age() const;
	void setAge(unsigned value);
	void incAge();

signals:
	void ageChanged(unsigned value);
public slots:
};
