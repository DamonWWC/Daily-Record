const http=require('http');
const url=require('url');

const app=http.createServer((req,res)=>{
    let urlObj=url.parse(req.url,true);
    
    switch(urlObj.pathname){
        case '/api/user':
            res.end(`${urlObj.query.callback}  (${JSON.stringify({
                name:"gp145",
                age:100
            })})`)
            break;
        default:
            res.end('404.')
            break;
    }
})

app.listen(8080,()=>{
    console.log('localhost:8080')
})