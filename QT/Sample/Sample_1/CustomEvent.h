#pragma once
#include <QEvent>
#include<QDebug>
#include<QWidget>
#include<QTimer>
#include <QApplication>

class CustomEvent :
    public QEvent
{
public:
    CustomEvent(int data):QEvent(QEvent::User),eventData(data){}

    int getData()const {
        return eventData;
    }

private:
    int eventData;
};

class SenderWidget :public QWidget {
public:
    SenderWidget() {
        QTimer::singleShot(2000, this, &SenderWidget::sendCustomEvent);
    }
    void sendCustomEvent() {
        CustomEvent *event=new CustomEvent(42);
        QCoreApplication::sendEvent(this, event);
    }
};

class ReceiveWidget :public QWidget {
public:
    ReceiveWidget() {
        installEventFilter(this);
    }
    bool event(QEvent* event) override {
        if (event->type() == QEvent::User) {
            CustomEvent* customEvent = static_cast<CustomEvent*>(event);
            int data = customEvent->getData();
            qDebug() << "Received custom event with data:" << data;
            return true;
        }
        return QWidget::event(event);
    }
};

