#include "qdialogsetheaders.h"
#include "ui_qdialogsetheaders.h"

QDialogSetHeaders::QDialogSetHeaders(QWidget *parent)
    : QDialog(parent)
    , ui(new Ui::QDialogSetHeaders)
{
    ui->setupUi(this);
    theModel=new QStringListModel(this);
    ui->listView->setModel(theModel);
}

QDialogSetHeaders::~QDialogSetHeaders()
{
    delete ui;
}

void QDialogSetHeaders::setStringList(QStringList strList)
{
    theModel->setStringList(strList);
}

QStringList QDialogSetHeaders::stringList()
{
    return theModel->stringList();
}
