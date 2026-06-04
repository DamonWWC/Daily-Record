#ifndef TRAYICON_H
#define TRAYICON_H
// /**
//  *托盘控件
//  *
//  * /
#include <QObject>
#include <QSystemTrayIcon>

class QMenu;

#ifdef quc
class Q_DECL_EXPORT trayicon : public QObject
#else
class TrayIcon : public QObject
#endif
{
    Q_OBJECT
public:
    static TrayIcon *Instance();
    explicit TrayIcon(QObject *parent = nullptr);
private:
    static QScopedPointer<TrayIcon> self;//智能指针类，自动管理内存
    QWidget *mainWidget;
    QSystemTrayIcon *trayIcon;
    QMenu *menu;
    bool exitDirect;
private slots:
    void iconIsActived(QSystemTrayIcon::ActivationReason reason);
public:
    void setExitDirect(bool exitDirect);

    void setMainWidget(QWidget *mainWidget);

    void showMessage(const QString &title,const QString &msg,
                     QSystemTrayIcon::MessageIcon icon=QSystemTrayIcon::Information,int msecs=5000);

    void setIcon(const QString &strIcon);

    void setToolTip(const QString &tip);

    bool getVisible()const;
    void setVisible(bool visible);
public Q_SLOTS:
    void closeAll();
    void showMainWidget();

signals:
    void trayIconExit();
};

#endif // TRAYICON_H
