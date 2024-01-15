function animation(duration,from,to,onProgress){
    const speed=(to - from)/duration;
    const starttime=Date.now();
    let value=from;
    function _run(){
        const now=Date.now();
        const time =now -starttime;
        if(time>=duration)
        {
            value=to;
            console.log(value);
            onProgress&&onProgress(value)
            return;
        }
        value=from + speed *time;
        console.log(value);
        onProgress&&onProgress(value)
        requestAnimationFrame(_run);
    }
    _run();
}

// requestAnimationFrame就是为了解决上述问题出现的：该接口以浏览器的显示频率来作为其动画动作的频率，
// 比如浏览器每10ms刷新一次，动画回调也每10ms调用一次，这样就不会存在过度绘制的问题，动画不会掉帧，自然流畅。