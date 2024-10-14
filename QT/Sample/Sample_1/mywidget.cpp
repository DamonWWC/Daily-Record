#include "mywidget.h"
#include "ui_mywidget.h"
#include<QDebug>
#include<QKeyEvent>
MyWidget::MyWidget(QWidget *parent)
    : QWidget(parent)
    , ui(new Ui::MyWidget)
{
    ui->setupUi(this);
}

MyWidget::~MyWidget()
{
    delete ui;
}
void MyWidget::keyPressEvent(QKeyEvent* event)
{
    qDebug() << "Esc pressed";
    if (event->key() == Qt::Key_Escape ){
        qDebug() << "Esc pressed";
    }
}
void MyWidget::mousePressEvent(QMouseEvent* event) 
{
    qDebug() << "Mouse pressed at(" << event->x() << "," << event->y() << ")";
}