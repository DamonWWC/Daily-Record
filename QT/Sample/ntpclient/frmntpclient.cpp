#include "frmntpclient.h"
#include "ui_frmntpclient.h"
#include "ntpclient.h"
#include <QDebug>
frmntpclient::frmntpclient(QWidget *parent)
    : QWidget(parent)
    , ui(new Ui::frmntpclient)
{
    ui->setupUi(this);
    ui->txtNtpIP->setText("ntp1.aliyun.com");
    connect(NtpClient::Instance(),SIGNAL(receiveTime(QDateTime)),this,SLOT(receiveTime(QDateTime)));
}

frmntpclient::~frmntpclient()
{
    delete ui;
}

void frmntpclient::on_btnGetTime_clicked()
{
    NtpClient::Instance()->setNtpIP(ui->txtNtpIP->text().trimmed());
    NtpClient::Instance()->getDateTime();
}
void frmntpclient::receiveTime(const QDateTime &dateTime)
{
    ui->txtTime->setText(dateTime.toString("yyyy-MM-dd HH:mm:ss zzz"));
}
