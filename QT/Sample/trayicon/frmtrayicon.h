#ifndef FRMTRAYICON_H
#define FRMTRAYICON_H

#include <QWidget>

QT_BEGIN_NAMESPACE
namespace Ui {
class frmtrayicon;
}
QT_END_NAMESPACE

class frmtrayicon : public QWidget
{
    Q_OBJECT

public:
    frmtrayicon(QWidget *parent = nullptr);
    ~frmtrayicon();

private slots:
    void on_pushButton_clicked();

    void on_pushButton_2_clicked();
    void cloeAll();

private:
    Ui::frmtrayicon *ui;
};
#endif // FRMTRAYICON_H
