#ifndef WIDGET_H
#define WIDGET_H

#include <QWidget>
#include"QPerson.h"
QT_BEGIN_NAMESPACE
namespace Ui {
class Widget;
}
QT_END_NAMESPACE

class Widget : public QWidget
{
    Q_OBJECT
private:
    QPerson *boy;
    QPerson *girl;
public:
    Widget(QWidget *parent = nullptr);
    ~Widget();

private:
    Ui::Widget *ui;
    
    void on_ageChanged(unsigned value);
    private slots:
        void on_boyinfo_clicked();
        void on_girlinfo_clicked();
        void on_btnclassinfo_clicked();
        void on_spin_valueChanged(int arg1);
};
#endif // WIDGET_H
