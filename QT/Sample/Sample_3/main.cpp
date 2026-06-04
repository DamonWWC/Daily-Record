#pragma execution_character_set("GB18030")

#include "widget.h"
#include <QApplication>
#include <QTextCodec>

int main(int argc, char *argv[])
{
    QApplication a(argc, argv);
    QFont font;
    font.setFamily("Microsoft Yahei");
    font.setPixelSize(13);
    a.setFont(font);

#if (QT_VERSION<QT_VERSION_CHECK(5,0,0))
#if _MSC_VER
    QTextCodec *codec= QTextCodec::codecForName("gbk");
#else
    QTextCodec* codec = QTextCodex::codecForName("UTF-8");
#endif
    QTextCodec::setCodecForLocale(codec);
    QTextCodec::setCodecForCString(codec);
    QTextCodec::setCodecForTr(codec);
#else
    QTextCodec *codec= QTextCodec::codecForName("GB18030");
    QTextCodec::setCodecForLocale(codec);
#endif

   
    Widget w;
    w.setWindowTitle(QString::fromLocal8Bit("ÊÓÆµ¼à¿Ø²¼¾Ö"));
    w.resize(800, 600);
    w.show();
    return a.exec();
}
