#ifndef WIDGET1_H
#define WIDGET1_H

#include <QWidget>
#include<QStringListModel>
namespace Ui {
class widget1;
}

class widget1 : public QWidget
{
    Q_OBJECT
private:
    QStringListModel *theModel;
public:
    explicit widget1(QWidget *parent = nullptr);
    ~widget1();

private slots:
    void on_pushButton_6_clicked();

    void on_pushButton_7_clicked();

    void on_pushButton_2_clicked();

    void on_pushButton_3_clicked();

private:
    Ui::widget1 *ui;
};

#endif // WIDGET1_H
