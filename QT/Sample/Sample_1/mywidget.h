#ifndef MYWIDGET_H
#define MYWIDGET_H

#include <QWidget>
#include<QKeyEvent>

QT_BEGIN_NAMESPACE
namespace Ui {
class MyWidget;
}
QT_END_NAMESPACE

class MyWidget : public QWidget
{
    Q_OBJECT

public:
    MyWidget(QWidget *parent = nullptr);
    ~MyWidget();
    void keyPressEvent(QKeyEvent* event);
signals:
    void mySignal(int value);

public slots:
    void mySlot(int value) {
        qDebug() << "Receviced value:" << value;
    }
public slots:
    void triggerSignal()
    {
        emit mySignal(42);
    }
private:
    Ui::MyWidget *ui;
protected:
    void mousePressEvent(QMouseEvent* event) override;

protected:
    bool event(QEvent* event) override {
        if (event->type() == QEvent::MouseButtonPress) {
            QMouseEvent* mouseEvent = static_cast<QMouseEvent*>(event);
            if (mouseEvent->button() == Qt::LeftButton) {
                qDebug() << "Left mouse button pressed at (" << mouseEvent->x() << "," << mouseEvent->y() << ")";
                return true;
            }
        }
        return QWidget::event(event);
    }
    //{
    //    // �жϰ��µ��Ƿ���������
    //    if (event->button() == Qt::LeftButton)
    //    {
    //        this->setWindowTitle("������������");
    //    }
    //    // ���ø����mousePressEvent��������ȷ���������ܲ���Ӱ��
    //    QWidget::mousePressEvent(event);
    //}
};
#endif // MYWIDGET_H
