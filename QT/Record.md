1. 信号：各种事件。槽：事件发生后执行的动作。当某个事件发生后，如某个按钮被点击了一下，它就会发出一个被点击的信号（signal）。某个对象接收到这个信号之后，就会做出一些相关的处理操作（称为槽slot）。要想一个对象接收到另一个对象发出的信号，就需要使用connect()函数，建立连接。参数样式connect(sender,signal,receiver,slot);sender:信号发送者,signal：信号，receiver：信号接收者，slot：接收对象在接收到信号之后所需要调用的函数（槽），这四个参数都是指针，信号和槽是函数指针，使用时保留&符号。
2. 元对象系统（Meta Object System）:QObject类是所有使用元对象系统的类的基类。在一个类的private部分声明Q_OBJECT宏。MOC(元对象编译器)为每个QOBject的子类提供必要的代码。
3. 属性系统：Q_PROPERTY宏定义一个返回类型为type，名称为name的属性。
4. 容器类：顺序容器通过元素在容器中的位置顺序存储和访问，如：QList、QLinkedList、QVector、QStack、QQueue。关联容器通过键存储和读取元素,如：QMap、QHash、QMultiMap、QMultiHash、QSet。