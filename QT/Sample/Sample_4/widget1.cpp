#include "widget1.h"
#include "ui_widget1.h"

widget1::widget1(QWidget *parent)
    : QWidget(parent)
    , ui(new Ui::widget1)
{
    ui->setupUi(this);
    theModel=new QStringListModel(this);
    QStringList strlist;
    strlist<<"item1"<<"item2"<<"item3"<<"item4"<<"item5"<<"item6";
    theModel->setStringList(strlist);
    ui->listView->setModel(theModel);
    ui->listView->setEditTriggers(QAbstractItemView::DoubleClicked|QAbstractItemView::SelectedClicked);
}

widget1::~widget1()
{
    delete ui;
}

void widget1::on_pushButton_6_clicked()
{
    ui->plainTextEdit->clear();
}


void widget1::on_pushButton_7_clicked()
{
    QStringList strlist=theModel->stringList();
    foreach(auto str,strlist){
        ui->plainTextEdit->appendPlainText(str);
    }
}

//插入
void widget1::on_pushButton_2_clicked()
{
    theModel->insertRow(theModel->rowCount());
    QModelIndex index=theModel->index(theModel->rowCount()-1,0);
    theModel->setData(index,"new item",Qt::DisplayRole);
    ui->listView->setCurrentIndex(index);
}


void widget1::on_pushButton_3_clicked()
{

    QModelIndex index=ui->listView->currentIndex();
    theModel->insertRow(index.row());

    theModel->setData(index,"inserted item",Qt::DisplayRole);
    ui->listView->setCurrentIndex(index);
}

