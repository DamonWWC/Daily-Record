window.addEventListener('click',(e)=>{
    const pointer=document.createElement('div');
    pointer.classList.add('pointer');
    pointer.style.left=`${e.clientX}px`;
    pointer.style.top=`${e.clientY}px`;
    document.body.appendChild(pointer);
    pointer.addEventListener('animationend',()=>{
        pointer.remove();
    });
});
const ball=document.querySelector('.ball');

function init(){
    const x=window.innerWidth / 2;
    const y=window.innerHeight / 2;
    ball.style.transform=`translate(${x}px,${y}px)`;
}
init();


window.addEventListener('click',e=>{
    const x=e.clientX;
    const y=e.clientY;
    ball.style.transform=`translate(${x}px,${y}px)`;
    move(x,y);
});

function move(x,y){
    const rect=ball.getBoundingClientRect();
    const ballx=rect.left + rect.width/2;
    const bally=rect.top + rect.height/2;

    ball.getAnimations().forEach(anim=>{
        anim.cancel();
    });

    const rac=Math.atan2(y-bally,x-ballx);
    const deg=(rac*180)/Math.PI;
    console.log(deg);
    ball.animate([
    {
        transform:`translate(${ballx}px,${bally}px) rotate(${deg}deg)`,
    },
    {
        transform:`translate(${ballx}px,${bally}px) rotate(${deg}deg) scaleX(1.5) ` ,
        offset:0.6,
    },
    {
        transform:`translate(${x}px,${y}px) rotate(${deg}deg) scaleX(1.5) `,
        offset:0.8,
    },
    {
        transform:`translate(${x}px,${y}px) rotate(${deg}deg)`,
        offset:1
    }
],
{
    duration:1000,
    fill:'forwards'
}
);
}
