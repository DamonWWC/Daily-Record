#include "BALL.h"
#include <iostream>
#include<graphics.h>
#include <ctime>

const int SCR_W = 1024;
const int SCR_H = 600;
const int MAP_W = SCR_W*2;
const int MAP_H = SCR_H*2;
const int FD_NUM = 200;

struct Ball 
{
	double x, y;
	double r;
	bool flag;
	COLORREF color;

};
Ball FD_Ball[FD_NUM];
Ball PL_Ball;
IMAGE MAP(MAP_W, MAP_H);
void SCR_Draw();
void MAP_Draw();
void GAME_Draw();
void FD_Init();
void FD_Draw();
void GAME_Init(); void PL_Init();
void PL_Draw();
int main()
{
	//绘制屏幕窗口
	initgraph(SCR_W, SCR_H);
	GAME_Init();
	GAME_Draw();
	

	/*setfillcolor(YELLOW);
	solidcircle(50, 250, 50);*/
	std::cin.get();

	return 0;
}
//屏幕窗口绘制
void SCR_Draw()
{
	SetWorkingImage();                //设置当前绘图区为窗口

	putimage(0, 0, SCR_W, SCR_H, &MAP, 0, 0);
}

void FD_Draw() {
	for (int i = 0; i < FD_NUM; i++)
	{
		if (FD_Ball[i].flag)
		{
			setfillcolor(FD_Ball[i].color);
			solidcircle(FD_Ball[i].x, FD_Ball[i].y, FD_Ball[i].r);
		}
	}
}

void PL_Draw()
{
	if (PL_Ball.flag)
	{
		setfillcolor(PL_Ball.color);
		solidcircle(PL_Ball.x, PL_Ball.y, PL_Ball.r);
	}
}
//地图绘制
void MAP_Draw()
{
	//绘制地图
	SetWorkingImage(&MAP);                //设置当前绘图区为MAP
	setbkcolor(WHITE);                    //设置背景色为白色
	cleardevice();                        //使用当前设置背景色清空图片

}

void GAME_Draw()
{
	MAP_Draw();
	SCR_Draw();	
	FD_Draw();	
	PL_Draw();
}

void FD_Init()
{
	std::srand(time(0));
	for (int i = 0; i < FD_NUM; i++)
	{		
		FD_Ball[i].x = std::rand() % SCR_W;
		FD_Ball[i].y = std::rand() % SCR_H;
		FD_Ball[i].r = std::rand() % 10 + 1;
		FD_Ball[i].color = RGB(rand() % 255, rand() % 255, rand() % 255);
		FD_Ball[i].flag = true;
	}	
}
void GAME_Init()
{
	std::srand(time(0));
	FD_Init();

	PL_Init();
}
void PL_Init()
{
	PL_Ball.x = std::rand() % SCR_W;
	PL_Ball.y=std::rand() % SCR_H;
	PL_Ball.r = 13;
	PL_Ball.color = RGB(rand() % 255, rand() % 255, rand() % 255);
	PL_Ball.flag = true;
}