#ifndef QDIALOGSETHEADERS_H
#define QDIALOGSETHEADERS_H

#include <QDialog>
#include<QStringListModel>
namespace Ui {
class QDialogSetHeaders;
}

class QDialogSetHeaders : public QDialog
{
    Q_OBJECT
private:
    QStringListModel *theModel;
public:
    explicit QDialogSetHeaders(QWidget *parent = nullptr);
    ~QDialogSetHeaders();

    void setStringList(QStringList qstringlist);
    QStringList stringList();


private:
    Ui::QDialogSetHeaders *ui;
};

#endif // QDIALOGSETHEADERS_H
