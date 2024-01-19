# import sys
# from PyQt5.QtWidgets import QMainWindow,QApplication,QWidget
# from Ui_untitled import Ui_MainWindow  #导入你写的界面类
 
 
# # class MyMainWindow(QMainWindow,Ui_MainWindow): #这里也要记得改
# #     def __init__(self,parent =None):
# #         super(MyMainWindow,self).__init__(parent)
# #         self.setupUi(self)
 
# # if __name__ == "__main__":
# #     app = QApplication(sys.argv)
# #     myWin = MyMainWindow()
# #     myWin.show()
# #     sys.exit(app.exec_())    


# def __init__(self):
#         super(QMainWindow, self).__init__()
#         self.setWindowTitle("打开网页例子")
#         #相当于初始化这个加载web的控件
#         self.browser = QWebEnginerView()
#         #加载外部页面，调用
#         self.browser.load(QUrl("http://www.baidu.com"))
#         self.setCentraWidget(self.browser)
# if __name__=='__main__':
#     app = QApplication(sys.argv)
#     win = MainWindow()
#     win.show()
#     sys.exit(spp.exec_())



# from PyQt5 import QtWidgets, QtWebEngineWidgets
# from PyQt5.QtCore import QUrl
# from Ui_untitled import Ui_MainWindow

 
 
# class MainWindow(QtWidgets.QMainWindow,Ui_MainWindow):
#     def __init__(self, parent=None):
#         super(MainWindow, self).__init__(parent)
#         self.setupUi(self)
 
#         #   --------------- start ---------------------
 
#         self.webEngineView = QtWebEngineWidgets.QWebEngineView(self.frame)
#         url = 'https://www.baidu.com/'
#         self.webEngineView.load(QUrl(url))
#         self.webEngineView.setGeometry(0, 0, 1063, 682)
 
#         #   --------------- end -----------------------
 
 
# if __name__ == "__main__":
#     import sys
 
#     app = QtWidgets.QApplication(sys.argv)
#     mainWindow = MainWindow()
#     mainWindow.show()
#     sys.exit(app.exec_())



# -*- coding: utf-8 -*-
# @Author: liyl
# @Date  :  2020/08/01
 
import sys
 
from PyQt5 import QtCore
from PyQt5.QtWidgets import *
from PyQt5.QtCore import *
from PyQt5.QtGui import *
from PyQt5.QtWebEngineWidgets import QWebEngineView
 
 
# 创建主窗口
# from main import WebEngineView
 
 
class MainWindow(QMainWindow):
    def __init__(self, *args, **kwargs):
        super().__init__(*args, **kwargs)
        # 设置窗口标题
        self.setWindowTitle('浏览器')
        # 设置窗口大小900*600
        self.resize(1300, 700)
        self.show()
 
        # 当前view加载地址
 
        self.webview = WebEngineView(self)
        self.webview.setUrl(QUrl("http://www.baidu.com"))
        self.setCentralWidget(self.webview)
 
        # 使用QToolBar创建导航栏，并使用QAction创建按钮
        # 添加导航栏
        navigation_bar = QToolBar('Navigation')
        # 设定图标的大小
        navigation_bar.setIconSize(QSize(16, 16))
        # 添加导航栏到窗口中
        self.addToolBar(navigation_bar)
 
        # 添加URL地址栏
        self.urlbar = QLineEdit()
        # 让地址栏能响应回车按键信号
        self.urlbar.returnPressed.connect(self.navigate_to_url)
        navigation_bar.addSeparator()
        navigation_bar.addWidget(self.urlbar)
 
        # 让浏览器相应url地址的变化
        self.webview.urlChanged.connect(self.renew_urlbar)
 
    # 显示地址
    def navigate_to_url(self):
        q = QUrl(self.urlbar.text())
        if q.scheme() == '':
            q.setScheme('http')
 
        self.webview.setUrl(q)
 
    # 响应输入的地址
    def renew_urlbar(self, q):
        # 将当前网页的链接更新到地址栏
        self.urlbar.setText(q.toString())
        self.urlbar.setCursorPosition(0)
 
# 第一种，是直接在本窗口新建tab的方式。 （不推荐使用这种方式）
# 注：这种方式有个问题，因为新建的tab覆盖了原来的tab，所以，原来tab的所有信息都找不到了，如浏览，账号，密码等。
class WebEngineView(QWebEngineView):
  # 重写createwindow()
  def createWindow(self, QWebEnginePage_WebWindowType):
    return self
 
# 第二种，就是新建窗口
# class WebEngineView(QWebEngineView):
#     windowList = []
#
#     # 重写createwindow()
#     def createWindow(self, QWebEnginePage_WebWindowType):
#         new_webview = WebEngineView()
#         new_window = MainWindow()
#         new_window.setCentralWidget(new_webview)
#         # new_window.show()
#         self.windowList.append(new_window)  # 注：没有这句会崩溃！！！
#         return new_webview
 
# 程序入口
if __name__ == "__main__":
    app = QApplication(sys.argv)
    # 创建主窗口
    browser = MainWindow()
    browser.show()
    # 运行应用，并监听事件
    sys.exit(app.exec_())