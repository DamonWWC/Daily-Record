#include "Draw.h"

Draw::Draw()
{
	MAP = IMAGE(1024 * 2, 600 * 2);
}
void Draw::MAP_Draw()
{
	//���Ƶ�ͼ
	SetWorkingImage(&MAP);                //���õ�ǰ��ͼ��ΪMAP
	setbkcolor(WHITE);                    //���ñ���ɫΪ��ɫ
	cleardevice();                        //ʹ�õ�ǰ���ñ���ɫ���ͼƬ
}
void Draw::SCR_Draw()
{

}