#include "widget.h"
#include "ui_widget.h"
#include <QPushButton>
#include <QSignalMapper>
#include <QDateTime>
#include<QDebug>
#define TIMEMS QTime::currentTime().toString("hh:mm:ss zzz");
Widget::Widget(QWidget *parent)
    : QWidget(parent)
    , ui(new Ui::Widget)
{
    ui->setupUi(this);
    this->initBtn();
}

Widget::~Widget()
{
    delete ui;
}

void Widget::initBtn()
{
    QSignalMapper *signMap=new QSignalMapper(this);
    connect(signMap,SIGNAL(mapped(QString)),this,SLOT(QString));

    int x=5,y=-25;
    for(int i=0;i<1000;++i)
    {
        x+=80;
        if(i%10==0){
            x=5;
            y+=30;
        }
        QPushButton *btn=new QPushButton(this);
        btn->setObjectName(QString("btn_%1").arg(i+1));
        btn->setText(QString("text_%1").arg(i+1));
        btn->setGeometry(x,y,75,25);

        //方法1：绑定到一个槽函数
        connect(btn,SIGNAL(clicked()),this,SLOT(doBtn()));
        //方法2：通过QSignalMapper转发信号
        // connect(btn,SIGNAL(clicked(bool)),signMap,SLOT(map()));
        // signMap->setMapping(btn,btn->objectName());
        //方法3:用lambda表达式
// #if(QT_VERSION>=QT_VERSION_CHECK(5,6,0))
//         connect(btn,&QPushButton::clicked,[btn]{
// QString name=btn->objectName();
// qDebug()<<QTime::currentTime().toString("hh:mm:ss zzz") << "doBtn3"<<name;
//         });
//         connect(btn, &QPushButton::clicked, [=]() {
//             QString name = btn->objectName();
//             qDebug() << QTime::currentTime().toString("hh:mm:ss zzz") << "doBtn3" << name;
//         });
// #endif
    }
}

void Widget::doBtn()
{
    QPushButton *btn=(QPushButton *) sender();
    QString name=btn->objectName();
    qDebug() << QTime::currentTime().toString("hh:mm:ss zzz") << "doBtn1" << name;
}
void Widget::doBtn(const QString &name)
{
     qDebug() << QTime::currentTime().toString("hh:mm:ss zzz") << "doBtn2" << name;
}
