#include "widget.h"
#include "ui_widget.h"

Widget::Widget(QWidget *parent)
    : QWidget(parent)
    , ui(new Ui::Widget)
{
    ui->setupUi(this);
    ui->spinboxboy->setProperty("isBoy", true);
    ui->spinBoxgirl->setProperty("isBoy", false);

    boy=new QPerson("boy",this);
    boy->setProperty("score", 79);
    boy->setProperty("age", 10);
    boy->setProperty("sex", "boy");
    connect(boy, &QPerson::ageChanged, this, &Widget::on_ageChanged);

    girl=new QPerson("girl",this);
    girl->setProperty("score", 79);
    girl->setProperty("age", 10);
    girl->setProperty("sex", "boy");
    connect(girl, &QPerson::ageChanged, this, &Widget::on_ageChanged);

    connect(ui->spinboxboy, SIGNAL(valueChanged(int)), this, SLOT(on_spin_valueChanged(int)));
    connect(ui->spinBoxgirl, SIGNAL(valueChanged(int)), this, SLOT(on_spin_valueChanged(int)));
}

Widget::~Widget()
{
    delete ui;
}

void Widget::on_boyinfo_clicked()
{
    boy->incAge();
}
void Widget::on_girlinfo_clicked()
{
    girl->incAge();
}
void Widget:: on_btnclassinfo_clicked()
{

}
void Widget::on_spin_valueChanged(int arg1)
{
    Q_UNUSED(arg1);
    QSpinBox *spinBox = qobject_cast<QSpinBox*>(sender());
    if (spinBox->property("isBoy").toBool())
        boy->setAge(spinBox->value());
    else
        girl->setAge(spinBox->value());
}
void Widget::on_ageChanged(unsigned value)
{
    Q_UNUSED(value);
    QPerson* aPerson = qobject_cast<QPerson*>(sender());
    QString aName=aPerson->property("name").toString();
    QString aSex=aPerson->property("sex").toString();
    unsigned aAge = aPerson->age();

    ui->plainTextEdit->appendPlainText(aName+","+aSex+ QString::asprintf(",ƒÍ¡‰=%d",aAge).toUtf8());
}
