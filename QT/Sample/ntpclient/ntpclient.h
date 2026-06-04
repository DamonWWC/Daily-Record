#ifndef NTPCLIENT_H
#define NTPCLIENT_H

#include <QObject>
#include<QDateTime>
class QUdpSocket;

#ifdef quc
class Q_DECL_EXPORT NtpClient:public QObject
#else
class NtpClient : public QObject
#endif
{
    Q_OBJECT
public:
    static NtpClient *Instance();
    explicit NtpClient(QObject *parent = nullptr);
private:
    static QScopedPointer<NtpClient>self;
    QString ntpIP;
    QUdpSocket *udpSocket;
private slots:
    void readData();
    void sendData();
    void setTime_t(uint secsSinceJan1970UTC);
public Q_SLOTS:
    void setNtpIP(const QString &ntpIP);
    void getDateTime();
Q_SIGNALS:
    void receiveTime(const QDateTime &dateTime);
};

#endif // NTPCLIENT_H
