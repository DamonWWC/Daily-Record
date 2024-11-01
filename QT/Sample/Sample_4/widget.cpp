#include "widget.h"
#include "ui_widget.h"

Widget::Widget(QWidget *parent)
    : QWidget(parent)
    , ui(new Ui::Widget)
{
    ui->setupUi(this);
    fileModel=new QFileSystemModel(this);
    fileModel->setRootPath(QDir::currentPath());
    ui->treeView->setModel(fileModel);
    ui->listView->setModel(fileModel);
    ui->tableView->setModel(fileModel);
    ui->tableView->verticalHeader()->setVisible(false);

}

Widget::~Widget()
{
    delete ui;
}

void Widget::on_treeView_clicked(const QModelIndex &index)
{
    ui->labName->setText(fileModel->fileName(index));

    ui->label_2->setText(fileModel->filePath(index));
    ui->labType->setText(fileModel->type(index));
    unsigned sz= fileModel->size(index)/1024;
    if(sz<1024)
        ui->labSize->setText(QString::asprintf("%d KB",sz));
    else
        ui->labSize->setText(QString::asprintf("%.2f MB",(float)sz/1024));

    ui->checkBox->setChecked(fileModel->isDir(index));
}

