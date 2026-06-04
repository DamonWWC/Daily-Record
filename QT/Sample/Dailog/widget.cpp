#include "widget.h"
#include "ui_widget.h"
#include<QInputDialog>
Widget::Widget(QWidget *parent)
    : QWidget(parent)
    , ui(new Ui::Widget)
{
    ui->setupUi(this);
}

Widget::~Widget()
{
    delete ui;
}

void Widget::on_pushButton_clicked()
{
    QString dlgTitle="输入整数对话框";
    QString txtLabel="请输入整数";
    bool ok=false;
    int value=QInputDialog::getInt(this,dlgTitle,txtLabel,10,0,100,1,&ok);
    if(!ok) return;

}

#include<QMessageBox>
void Widget::on_pushButton_2_clicked()
{
    QString dlgTitle="Question信息框";
    QString textLabel="文件已经被修改";
    QMessageBox::StandardButton result;
    result=QMessageBox::question(this,dlgTitle,textLabel,
                                   QMessageBox::Yes|QMessageBox::No|QMessageBox::Cancel,QMessageBox::NoButton);
    if(result==QMessageBox::Yes)
    {

    }else if(result==QMessageBox::No)
    {

    }
    else
    {

    }
}

