const cvs=document.getElementById('bg');

const width=window.innerWidth * devicePixelRatio,
height=window.innerHeight * devicePixelRatio;
cvs.width=width;
cvs.height=height;

const ctx=cvs.getContext('2d');

const fontSize=20 * devicePixelRatio;
const columnWidth=fontSize;

const columnCount=Math.floor(width/columnWidth);

const nextChars=Array(columnCount).fill(0);

function draw(){
    ctx.fillStyle='rgba(0,0,0,0.1)';
    ctx.fillRect(0,0,width,height);
    for(let i=0;i<columnCount;i++)
    {
        const char=getRandomChar();
        ctx.fillStyle=getRandomColor();
        ctx.font=`${fontSize}px sans-serif`;
        const x=columnWidth*i;
        const index=nextChars[i];
        const y=(index + 1)*fontSize;
      // console.log(x,y);
        ctx.fillText(char,x,y);
        if(y>height && Math.random()>0.99){
            nextChars[i]=0;
        }
        else{
            nextChars[i]++;
        }
       
    }
}
function getRandomColor() {
    return `rgb(${Math.floor(Math.random() * 255)}, ${Math.floor(Math.random() * 255)}, ${Math.floor(Math.random() * 255)})`;
}

function getRandomChar(){
    const str='console.log("hello world")")';
    return str[Math.floor( Math.random() * str.length)];
}

draw();
setInterval(draw,30);