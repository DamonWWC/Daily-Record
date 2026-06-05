import mqtt from 'mqtt';

export default class MqttClass {
    constructor (connection, callback) {
        this.callback = callback;
        this.retryTimes = 0;
        this.connecting = false;
        this.subscribeSuccess = false;
        this.client = {
            connected: false
        };
        this.connection = connection;
    }

    // 连接
    connect () {
        try {
            this.connecting = true;
            const { protocol, host, port, endpoint, ...options } = this.connection;
            const connectUrl = `${protocol}://${host}:${port}${endpoint}`;
            this.client = mqtt.connect(connectUrl, options);
            if (this.client.on) {
                this.client.on('connect', () => {
                    this.connecting = false;
                    console.log('Connection succeeded!');
                });
                this.client.on('reconnect', this.handleOnReConnect);
                this.client.on('error', (error) => {
                    console.log('Connection failed', error);
                });
                this.client.on('message', (topic, message) => {
                    // this.receiveNews = this.receiveNews.concat(message);
                    console.log(`Received message ${message} from topic ${topic}`);

                    this.callback({ topic, message });
                });
            }
        } catch (error) {
            this.connecting = false;
            console.log('mqtt.connect error', error);
        }
    }

    // 消息发布
    doPublish (publish) {
        console.log('doPublish:', publish);
        const { topic, qos, payload } = publish;
        this.client.publish(topic, payload, { qos }, error => {
            if (error) {
                console.log('Publish error', error);
            }
        });
    }

    // 订阅主题
    doSubscribe (subscription) {
        const { topic, qos } = subscription;
        this.client.subscribe(topic, { qos }, (error, res) => {
            if (error) {
                console.log('Subscribe to topics error', error);
                return;
            }
            this.subscribeSuccess = true;
            console.log('Subscribe to topics res', res);
        });
    }

    // 取消订阅
    doUnSubscribe (subscription) {
        const { topic } = subscription;
        this.client.unsubscribe(topic, error => {
            if (error) {
                console.log('Unsubscribe error', error);
            }
        });
    }

    // 断开连接
    close () {
        this.destroyConnection();
    }

    destroyConnection () {
        if (this.client.connected) {
            try {
                this.client.end(false, () => {
                    this.initData();
                    console.log('Successfully disconnected!');
                });
            } catch (error) {
                console.log('Disconnect failed', error.toString());
            }
        }
    }

    // 重连
    handleOnReConnect () {
        this.retryTimes += 1;
        if (this.retryTimes > 5) {
            try {
                this.client.end();
                this.initData();
                console.log('Connection maxReconnectTimes limit, stop retry');
            } catch (error) {
                console.log(error.toString());
            }
        }
    }

    initData () {
        this.client = {
            connected: false
        };
        this.retryTimes = 0;
        this.connecting = false;
        this.subscribeSuccess = false;
    }
}
