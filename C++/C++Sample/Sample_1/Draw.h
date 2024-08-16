#pragma once
#include <iostream>
#include<graphics.h>
struct Ball
{
	double x, y;
	double r;
	bool flag;
	COLORREF color;
};
const int FD_NUM = 200;
class Draw
{
private:
	Ball FD_Ball[FD_NUM];
	IMAGE MAP;
	

public:
	Draw();
	void GAME_DRAE();
	void SCR_Draw();
	void MAP_Draw();
};

