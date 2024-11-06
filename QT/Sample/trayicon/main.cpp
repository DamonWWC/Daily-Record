#include "frmtrayicon.h"

#include <QApplication>

int main(int argc, char *argv[])
{
    QApplication a(argc, argv);
    frmtrayicon w;
    w.show();
    return a.exec();
}
