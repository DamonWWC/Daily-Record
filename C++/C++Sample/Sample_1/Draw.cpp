#include "Draw.h"

Draw::Draw()
{
	MAP = IMAGE(1024 * 2, 600 * 2);
}
void Draw::MAP_Draw()
{
	//绘制地图
	SetWorkingImage(&MAP);                //设置当前绘图区为MAP
	setbkcolor(WHITE);                    //设置背景色为白色
	cleardevice();                        //使用当前设置背景色清空图片
}
void Draw::SCR_Draw()
{

}