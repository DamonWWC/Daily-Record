#pragma once
#include<QTimer>
#include<QWidget>
#include<QLabel>

class TimerExample :public QWidget
{
	Q_OBJECT
		private:
			QTimer* timer;
			QLabel* label;
public:
	TimerExample(QWidget* parent = nullptr) :QWidget(parent) {
		timer = new QTimer(this);
		timer->setInterval(1000);

		label = new QLabel("Time 0", this);
		label->setGeometry(50, 50, 300, 30);

		connect(timer, SIGNAL(timeout()), this, SLOT(updateTime()));
		timer->start();
	}
public slots:
	void updateTime() {
		static int time = 0;
		label->setText("Time:" + QString::number(time));
		time++;
	}
};
