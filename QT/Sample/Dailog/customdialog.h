#ifndef CUSTOMDIALOG_H
#define CUSTOMDIALOG_H

#include <QWidget>
#include<QStandardItemModel>
#include<QItemSelectionModel>
#include"qdialogsetheaders.h"
namespace Ui {
class CustomDialog;
}

class CustomDialog : public QWidget
{
    Q_OBJECT
private:
    QStandardItemModel *theModel;
    QItemSelectionModel *theSelectionModel;
    QDialogSetHeaders *dialogHeader=NULL;
public:
    explicit CustomDialog(QWidget *parent = nullptr);
    ~CustomDialog();

private slots:
    void on_pushButton_clicked();

    void on_pushButton_2_clicked();

private:
    Ui::CustomDialog *ui;
};

#endif // CUSTOMDIALOG_H
