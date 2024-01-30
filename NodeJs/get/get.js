const http=require('http');
const https=require('https');
const url=require('url');

const app=http.createServer((req,res)=>{
    let urlObj=url.parse(req.url,true);
     res.writeHead(200,{
        "Content-Type":"application/json;charset=utf-8",
        "access-control-allow-origin":"*"
     })


    switch(urlObj.pathname){
        case '/api/user':
            httpget(res);
            break;
        default:
            res.end('404.')
            break;
    }
})

app.listen(8080,()=>{
    console.log('localhost:8080')
})

function httpget(response){
    let data=""
    https.get("https://i.maoyan.com/api/mmdb/movie/v3/list/hot.json?ct=%E5%B9%BF%E5%B7%9E&ci=20&channelId=4",(res)=>{
        res.on("data",(result)=>{
            data+=result
        })
        res.on("end",()=>{
            console.log(data)
            response.end(data)
        })
    })
}