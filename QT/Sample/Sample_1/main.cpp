#include "mywidget.h"

#include <QApplication>
#include<QPushButton>
#include<QLineEdit>
#include<student.h>
#include<teacher.h>
#include <QThread>
#include<QGridLayout>
#include<QWidget>
#include<QStackedLayout>
#include<QFormLayout>
#include<QLabel>
#include<EventFilter.h>
#include<TimerExample.h>
#include <QFile>
#include <QDataStream>
#include<QFileInfo>


int main(int argc, char *argv[])
{
    QApplication a(argc, argv);
    
   
   // MyWidget w;
   // QPushButton* Btn = new QPushButton("hello", &w);
   ///* w.connect(Btn, &QPushButton::clicked, [&](){
   //     Btn->setText("world");
   // });*/
   // QObject::connect(Btn, &QPushButton::clicked,&w,&MyWidget::close);
   // 
   // student *pStudent=new student(&w);
   // teacher *pTeacher=new teacher(&w);
   // //QObject::connect(pStudent, &student::hungry, pTeacher, &teacher::treat);

   // void(student:: * student_qstring)(QString) = &student::hungry;
   // void(teacher::* teacher_qstring)(QString) = &teacher::treat;
   // QObject::connect(pStudent, student_qstring, pTeacher, teacher_qstring);

   // pStudent->increment("sss");




   // QLineEdit *lineEdit=new QLineEdit(&w);
   // lineEdit->move(100,100);
   // QObject::connect(lineEdit, &QLineEdit::textChanged, [=](const QString& text) {
   //     qInfo() << "Text changed: " << text;
   //     });


   // w.show();


//#pragma region QGridLayout
//    QWidget *widget=new QWidget();
//    QGridLayout* layout = new QGridLayout(widget);
//
//    QPushButton *button1=new QPushButton("Button 1");
//    QPushButton *button2=new QPushButton("Button 2");
//    QPushButton *button3=new QPushButton("Button 3");
//
//    layout->addWidget(button1, 0, 0);
//    layout->addWidget(button2, 1, 1);
//    layout->addWidget(button3, 1,0,1, 2);
//
//    widget->setLayout(layout);
//    widget->show();
//
//#pragma endregion

//#pragma region QStackedLayout
//    QWidget* widget = new QWidget();
//    QStackedLayout *layout=new QStackedLayout(widget);
//
//    QPushButton *button1=new QPushButton("Page 1");
//    QPushButton *button2=new QPushButton("Page 2");
//    QPushButton *button3=new QPushButton("Page 3");
//
//    layout->addWidget(button1);
//    layout->addWidget(button2);
//    layout->addWidget(button3);
//
//    layout->setCurrentIndex(1);
//
//    widget->setLayout(layout);
//    widget->show();
//
//#pragma endregion
// 
// 



//������
   
   // MyWidget*widget=new MyWidget();
  /*  QFormLayout *layout=new QFormLayout(widget);

    QLabel *label1=new QLabel(QString::fromLocal8Bit("����:"));
    QLineEdit *lineEdit1=new QLineEdit();

    QLabel *label2=new QLabel(QString::fromLocal8Bit("����:"));
    QLineEdit *lineEdit2=new QLineEdit();

    layout->addRow(label1,lineEdit1);
    layout->addRow(label2,lineEdit2);

    widget->setLayout(layout);*/
    //widget->show();
//
    //�¼�������
 /*   QWidget window;
    window.setWindowTitle("Event Filter Example");
    window.setGeometry(100,100,300,300);

    QPushButton button("Click Me!", &window);
    button.setGeometry(100,50,100,30);

    EventFilter eventFilter;
    button.installEventFilter(&eventFilter);
    window.show();*/
    //

//TimerExample widget;
//widget.setWindowTitle("Timer Example");
//widget.setGeometry(100, 100, 300, 200);
//widget.show();

//�������ļ��Ķ�д


//QFile file("data.bin");
////д�����ݵ��������ļ�
//if (file.open(QIODevice::WriteOnly)) {
//    QDataStream out(&file);
//
//    int intValue = 42;
//    double doubleValue = 3.14;
//    QString stringValue = "Hello World";
//
//    out << intValue << doubleValue << stringValue;
//    file.close();
//}
//
////�Ӷ������ļ��ж�ȡ����
//if (file.open(QIODevice::ReadOnly)) {
//    QDataStream in(&file);
//    int intValue;
//    double doubleValue;
//    QString stringValue;
//
//    in >> intValue >> doubleValue >> stringValue;
//    file.close();
//
//    qDebug()<<"Read from file:"<<intValue<<doubleValue<<stringValue;
//}



QString filePath = "data.bin";
QFileInfo fileInfo(filePath);

qint64 size = fileInfo.size();
qDebug()<< "File size:" << size<<" bytes";

QDateTime createdTime = fileInfo.birthTime();
QDateTime modifiedTime = fileInfo.lastModified();

qDebug() << "Created Time: " << createdTime;
qDebug() << "Modified Time: " << modifiedTime;


if (fileInfo.exists())
{
    qDebug() << "File exists!";
}
else {
    qDebug() << "File does not exist!";
}


    return a.exec();
}
