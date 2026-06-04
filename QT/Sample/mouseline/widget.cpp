#include "widget.h"
#include "ui_widget.h"
#include <QPainter>
#include<QEvent>
#include<QDebug>
#include <QMouseEvent>


Widget::Widget(QWidget *parent)
    : QWidget(parent)
    , ui(new Ui::Widget)
{
    ui->setupUi(this);
    this->setMouseTracking(true);
}

Widget::~Widget()
{
    delete ui;
}

void Widget::mouseMoveEvent(QMouseEvent *event)
{
    lastpos=event->pos();
    update();
}

void Widget::mouseReleaseEvent(QMouseEvent *event)
{
    lastpos=event->pos();
    update();
    qDebug()<<lastpos;
}
void Widget::paintEvent(QPaintEvent *){
    QPainter painter(this);
    QPen pen;
    pen.setWidth(5);
    pen.setColor(Qt::red);
    painter.setPen(pen);

    painter.drawLine(0,lastpos.y(),width(),lastpos.y());

    painter.drawLine(lastpos.x(),0,lastpos.x(),height());
}
