#include "qdialogsetsize.h"
#include "ui_qdialogsetsize.h"
#include<QMessageBox>
QDialogSetSize::QDialogSetSize(QWidget *parent)
    : QDialog(parent)
    , ui(new Ui::QDialogSetSize)
{
    ui->setupUi(this);
}

QDialogSetSize::~QDialogSetSize()
{
    QMessageBox::information(this,"","退出对话框");
    delete ui;
}

int QDialogSetSize::columnCount()
{
    return ui->spinBox->value();
}

int QDialogSetSize::rowCount()
{
    return ui->spinBox_2->value();
}

void QDialogSetSize::SetRowsColumns(int row, int col)
{
    ui->spinBox->setValue(col);
    ui->spinBox_2->setValue(row);
}
