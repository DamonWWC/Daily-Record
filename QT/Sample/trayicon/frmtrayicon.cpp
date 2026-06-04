#include "frmtrayicon.h"
#include "ui_frmtrayicon.h"
#include "trayicon.h"
#include <QDebug>

frmtrayicon::frmtrayicon(QWidget *parent)
    : QWidget(parent)
    , ui(new Ui::frmtrayicon)
{
    ui->setupUi(this);
    TrayIcon::Instance()->setIcon(":/main.ico");
    TrayIcon::Instance()->setMainWidget(this);
    TrayIcon::Instance()->setExitDirect(false);
    connect(TrayIcon::Instance(),SIGNAL(trayIconExit()),this,SLOT(cloeAll()));
}

frmtrayicon::~frmtrayicon()
{
    TrayIcon::Instance()->setVisible(false);
    delete ui;
}

void frmtrayicon::on_pushButton_clicked()
{
    TrayIcon::Instance()->setVisible(true);
    TrayIcon::Instance()->showMessage("自定义控件大全","已经最小化到推盘，双击打开！");
}


void frmtrayicon::on_pushButton_2_clicked()
{
    TrayIcon::Instance()->setVisible(false);
}

void frmtrayicon::cloeAll()
{
    qDebug()<<"close";
}

