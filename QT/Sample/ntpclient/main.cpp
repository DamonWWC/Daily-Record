#include "frmntpclient.h"

#include <QApplication>

int main(int argc, char *argv[])
{
    QApplication a(argc, argv);
    frmntpclient w;
    w.setWindowTitle(QStringLiteral("Ntp校时"));
    w.show();
    return a.exec();
}
