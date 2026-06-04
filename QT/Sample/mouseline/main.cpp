#include "widget.h"

#include <QApplication>

int main(int argc, char *argv[])
{
    QApplication a(argc, argv);
    Widget w;
    w.setWindowTitle(QStringLiteral("鼠标十字"));
    w.show();
    return a.exec();
}
