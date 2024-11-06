#include "customdialog.h"
#include "ui_customdialog.h"


CustomDialog::CustomDialog(QWidget *parent)
    : QWidget(parent)
    , ui(new Ui::CustomDialog)
{
    ui->setupUi(this);
    theModel=new QStandardItemModel(this);
    theSelectionModel=new QItemSelectionModel(theModel);
    ui->tableView->setModel(theModel);
    ui->tableView->setSelectionModel(theSelectionModel);
}

CustomDialog::~CustomDialog()
{
    delete ui;
}
#include "qdialogsetsize.h"
#include<QDialog>
void CustomDialog::on_pushButton_clicked()
{
    QDialogSetSize *dlg=new QDialogSetSize(this);
    dlg->SetRowsColumns(theModel->rowCount(),theModel->columnCount());
    int ret=dlg->exec();
    if(ret==QDialog::Accepted){
        int col=dlg->columnCount();
        int row=dlg->rowCount();
        theModel->setColumnCount(col);
        theModel->setRowCount(row);
    }
    delete dlg;
}


void CustomDialog::on_pushButton_2_clicked()
{
    if(dialogHeader==NULL)
    {
         dialogHeader=new QDialogSetHeaders(this);
    }
    if(dialogHeader->stringList().count()!=theModel->columnCount()){
        QStringList strList;
        for(int i=0;i<theModel->columnCount();i++)
        {
            strList<<theModel->headerData(i,Qt::Horizontal).toString();
        }
        dialogHeader->setStringList(strList);
    }

    int ret=dialogHeader->exec();
    if(ret==QDialog::Accepted)
    {
        QStringList strList=dialogHeader->stringList();
        theModel->setHorizontalHeaderLabels(strList);
    }




}

