import { mapState, mapGetters } from 'vuex';

export default {
    computed: {
        ...mapGetters({ isConnected: 'mqtt/isConnected' }),
        ...mapState({
            mqttResponse: state => state.mqtt.response,
            wpfData: state => state.wpf.wpfData
        })
    },
    watch: {
        topic: {
            handler (newVal, oldVal) {
                console.log('topic changed', newVal, oldVal);
                oldVal && this.unsubscribe && this.unsubscribe(oldVal);
                if (newVal && this.isConnected) {
                    this.onConnected && this.onConnected();
                }
            }
        },
        mqttResponse: {
            immediate: true,
            deep: true,
            handler (val) {
                console.log('mqttResponse:', val);

                this.onResponse && this.onResponse(val);
            }
        },
        isConnected: {
            immediate: true,
            handler (val) {
                val && this.onConnected && this.onConnected();
            }
        }
    },
    beforeDestroy () {
        this.unsubscribe && this.unsubscribe(this.topic);
    }
};
