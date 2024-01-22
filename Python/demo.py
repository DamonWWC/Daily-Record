from PyQt5 import QtCore, QtGui, QtWidgets

class Ui_MainWindow(object):
    def setupUi(self, MainWindow):
        MainWindow.setObjectName("MainWindow")
        MainWindow.resize(800, 600)
        self.centralwidget = QtWidgets.QWidget(MainWindow)
        self.centralwidget.setObjectName("centralwidget")
        self.horizontalLayoutWidget = QtWidgets.QWidget(self.centralwidget)
        self.horizontalLayoutWidget.setGeometry(QtCore.QRect(10, 10, 781, 31))
        self.horizontalLayoutWidget.setObjectName("horizontalLayoutWidget")
        self.horizontalLayout = QtWidgets.QHBoxLayout(self.horizontalLayoutWidget)
        self.horizontalLayout.setContentsMargins(0, 0, 0, 0)
        self.horizontalLayout.setObjectName("horizontalLayout")
        self.label = QtWidgets.QLabel(self.horizontalLayoutWidget)
        self.label.setObjectName("label")
        self.horizontalLayout.addWidget(self.label)
        self.url_box = QtWidgets.QTextEdit(self.horizontalLayoutWidget)
        self.url_box.setObjectName("url_box")
        self.horizontalLayout.addWidget(self.url_box)
        self.go_Button = QtWidgets.QPushButton(self.horizontalLayoutWidget)
        self.go_Button.setObjectName("go_Button")
        self.horizontalLayout.addWidget(self.go_Button)
        self.horizontalLayoutWidget_2 = QtWidgets.QWidget(self.centralwidget)
        self.horizontalLayoutWidget_2.setGeometry(QtCore.QRect(10, 50, 781, 501))
        self.horizontalLayoutWidget_2.setObjectName("horizontalLayoutWidget_2")
        self.horizontalLayout_2 = QtWidgets.QHBoxLayout(self.horizontalLayoutWidget_2)
        self.horizontalLayout_2.setContentsMargins(0, 0, 0, 0)
        self.horizontalLayout_2.setObjectName("horizontalLayout_2")
        
        # 此处添加
        self.webview = QtWebEngineWidgets.QWebEngineView(self.horizontalLayoutWidget_2)
        sizePolicy = QtWidgets.QSizePolicy(QtWidgets.QSizePolicy.Preferred, QtWidgets.QSizePolicy.Preferred)
        sizePolicy.setHorizontalStretch(0)
        sizePolicy.setVerticalStretch(8)
        sizePolicy.setHeightForWidth(self.webview.sizePolicy().hasHeightForWidth())
        self.webview.setSizePolicy(sizePolicy)
        self.webview.setObjectName("webview")
        self.horizontalLayout_2.addWidget(self.webview)
        # 此处结束
        
        MainWindow.setCentralWidget(self.centralwidget)
        self.menubar = QtWidgets.QMenuBar(MainWindow)
        self.menubar.setGeometry(QtCore.QRect(0, 0, 800, 22))
        self.menubar.setObjectName("menubar")
        MainWindow.setMenuBar(self.menubar)
        self.statusbar = QtWidgets.QStatusBar(MainWindow)
        self.statusbar.setObjectName("statusbar")
        MainWindow.setStatusBar(self.statusbar)

        self.retranslateUi(MainWindow)
        QtCore.QMetaObject.connectSlotsByName(MainWindow)

    def retranslateUi(self, MainWindow):
        _translate = QtCore.QCoreApplication.translate
        MainWindow.setWindowTitle(_translate("MainWindow", "MainWindow"))
        self.label.setText(_translate("MainWindow", "输入网址："))
        self.go_Button.setText(_translate("MainWindow", "→"))
from PyQt5 import QtWebEngineWidgets

import sys

from PyQt5.QtCore import *
from PyQt5.QtWidgets import *
# from webtest import Ui_MainWindow

class MainWindow(QMainWindow):
    def __init__(self):
        super().__init__()
        self.setWindowTitle('加载外部网页的例子')
        self.__ui = Ui_MainWindow()
        self.__ui.setupUi(self)
        self.__ui.go_Button.clicked.connect(self.geturl)
        #回车键绑定按钮，但只能在文本框外进行
        # self.__ui.go_Button.setShortcut(Qt.Key_Return)

    def geturl(self):
        url = self.__ui.url_box.toPlainText()
        # 网页加载方法        
        self.__ui.webview.load(QUrl(url))

if __name__ == '__main__':
    app=QApplication(sys.argv)
    win=MainWindow()
    win.show()
    app.exit(app.exec())
