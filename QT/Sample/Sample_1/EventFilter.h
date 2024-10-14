#pragma once
#include<QApplication>
#include<QDebug>
class EventFilter :public QObject {
	Q_OBJECT
public:
	explicit EventFilter(QObject*parent=nullptr):QObject(parent){}
protected:
	bool eventFilter(QObject* watched, QEvent* event)override {
		if (watched->isWidgetType() && event->type() == QEvent::MouseButtonPress) {
			qDebug() << "Mouse button pressed on widget:" << watched;
			return true;//����true��ʾ�¼��Ѿ�������ϣ����ټ�������
		}
		return false;//����false��ʾ�¼�δ������������
	}
};