#ifndef FRMNTPCLIENT_H
#define FRMNTPCLIENT_H

#include <QWidget>

QT_BEGIN_NAMESPACE
namespace Ui {
class frmntpclient;
}
QT_END_NAMESPACE

class frmntpclient : public QWidget
{
    Q_OBJECT

public:
    frmntpclient(QWidget *parent = nullptr);
    ~frmntpclient();

private:
    Ui::frmntpclient *ui;

private slots:
    void on_btnGetTime_clicked();
    void receiveTime(const QDateTime &dateTime);
};
#endif // FRMNTPCLIENT_H
