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
			return true;//返回true表示事件已经处理完毕，不再继续传递
		}
		return false;//返回false表示事件未处理，继续传递
	}
};