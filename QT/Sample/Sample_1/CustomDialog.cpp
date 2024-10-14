////#include "CustomDialog.h"
//#include<QApplication>
//#include<QDialog>
//#include<QVBoxLayout>
//#include<QLabel>
//#include<QPushButton>
//
//class CustomDialog :public QDialog {
//	Q_OBJECT
//public:
//	CustomDialog(QWidget* parent = nullptr);
//	~CustomDialog();	
//};
//
//
//CustomDialog::CustomDialog(QWidget* parent):QDialog(parent) {
//
//	QVBoxLayout* layout=new QVBoxLayout(this);
//	QLabel* label = new QLabel("This is a custom dialog.", this);
//	layout->addWidget(label);
//
//	QPushButton * okButton=new QPushButton("OK", this);
//	layout->addWidget(okButton);
//
//	connect(okButton, SIGNAL(clicked()), this, SLOT(accept()));
//
//}