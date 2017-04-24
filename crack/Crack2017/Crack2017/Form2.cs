using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

// для работы с библиотекой OpenGL 
using Tao.OpenGl;
// для работы с библиотекой FreeGLUT 
using Tao.FreeGlut;
// для работы с элементом управления SimpleOpenGLControl 
using Tao.Platform.Windows;
using Engine;

namespace Crack2017
{
    public partial class Form2 : Form
    {
        Camera cam = new Camera();
        bool mouseRotate = false, mouseMove = false;
        int myMouseYcoord, myMouseXcoord, myMouseXcoordVar, myMouseYcoordVar, rot_cam_X;

        public Form2()
        {
            InitializeComponent();
            AnT.InitializeContexts();
        }
        private void InitGL()
        {

            // инициализация Glut 
            Glut.glutInit();
            // инициализация режима экрана 
            Glut.glutInitDisplayMode(Glut.GLUT_RGB | Glut.GLUT_DOUBLE | Glut.GLUT_DEPTH);

            // установка цвета очистки экрана (RGBA) 
            Gl.glClearColor(255, 255, 255, 1);

            // установка порта вывода в соответствии с размерами элемента anT 
            Gl.glViewport(0, 0, AnT.Width, AnT.Height);

            // активация проекционной матрицы 
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            // очистка матрицы 
            Gl.glLoadIdentity();

            // установка перспективы 
            Glu.gluPerspective(45, (float)AnT.Width / (float)AnT.Height, 0.1, 200);


            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            
            Gl.glLoadIdentity();

            // начальная настройка параметров openGL (тест глубины, освещение и первый источник света) 
            Gl.glEnable(Gl.GL_DEPTH_TEST);
            
            Gl.glDisable(Gl.GL_LIGHTING);
            Gl.glEnable(Gl.GL_LIGHT0);


            cam.Position_Camera(0, 6, -15, 0, 3, 0, 0, 1, 0); // Вот тут в инициализации
            // укажем начальную позицию камеры, взгляда и вертикального вектора.
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            InitGL();
        }
        private void DrawGrid(int x, float quad_size)
        {
            float[] MatrixColorOX = new float[] { 0.0f, 1.0f, 1.0f, 1.0f };
            float[] MatrixColorOY = new float[] { 0.3f, 1.0f, 1.0f, 1.0f };
            float[] MatrixColorOZ = new float[] { 0.1f, 0.2f, 1.0f, 0.9f };
            float[] MatrixOXOYColor = new float[] { 0.1f, 0.5f, 1.0f, 1.0f };
            // x - количество или длина сетки, quad_size - размер клетки

            Gl.glPushMatrix(); // Рисуем оси координат, цвет объявлен в самом начале
            Gl.glEnable(Gl.GL_COLOR_MATERIAL);
            Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_AMBIENT_AND_DIFFUSE, MatrixColorOX);
            Gl.glTranslated((-x * 2) / 2, 0, 0);
            Gl.glRotated(90, 0, 1, 0);
            
            Glut.glutSolidCylinder(0.02, x * 2, 12, 12);
            Gl.glPopMatrix();

            Gl.glPushMatrix();
            Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_AMBIENT_AND_DIFFUSE, MatrixColorOZ);
            Gl.glTranslated(0, 0, (-x * 2) / 2);
            Glut.glutSolidCylinder(0.02, x * 2, 12, 12);
            Gl.glPopMatrix();

            Gl.glPushMatrix();
            Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_AMBIENT_AND_DIFFUSE, MatrixColorOY);
            Gl.glTranslated(0, x / 2, 0);
            Gl.glRotated(90, 1, 0, 0);
            Glut.glutSolidCylinder(0.02, x, 12, 12);
            Gl.glPopMatrix();

            Gl.glBegin(Gl.GL_LINES);

            Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_AMBIENT_AND_DIFFUSE, MatrixOXOYColor);

            // Рисуем сетку 1х1 вдоль осей
            for (float i = -x; i <= x; i += 1)
            {
                Gl.glBegin(Gl.GL_LINES);
                // Ось Х
                Gl.glVertex3f(-x * quad_size, 0, i * quad_size);
                Gl.glVertex3f(x * quad_size, 0, i * quad_size);

                // Ось Z
                Gl.glVertex3f(i * quad_size, 0, -x * quad_size);
                Gl.glVertex3f(i * quad_size, 0, x * quad_size);
                Gl.glEnd();
            }
            /*Gl.glColor3f(.3f, .3f, .3f);
            Gl.glBegin(Gl.GL_QUADS);
            Gl.glVertex3f(0, -0.001f, 0);
            Gl.glVertex3f(0, -0.001f, 10);
            Gl.glVertex3f(10, -0.001f, 10);
            Gl.glVertex3f(10, -0.001f, 0);
            Gl.glEnd();

            Gl.glBegin(Gl.GL_LINES);
            for (int i = 0; i <= 10; i++)
            {
                if (i == 0) { Gl.glColor3f(.6f, .3f, .3f); } else { Gl.glColor3f(.25f, .25f, .25f); };
                Gl.glVertex3f(i, 0, 0);
                Gl.glVertex3f(i, 0, 10);
                if (i == 0) { Gl.glColor3f(.3f, .3f, .6f); } else { Gl.glColor3f(.25f, .25f, .25f); };
                Gl.glVertex3f(0, 0, i);
                Gl.glVertex3f(10, 0, i);
            };
            Gl.glEnd();*/
        }
        private void Draw()
        {
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);

            Gl.glLoadIdentity();
            Gl.glColor3i(255, 0, 0);

            cam.Look(); // Обновляем взгляд камеры

            Gl.glPushMatrix();

            DrawGrid(30, 1); // Нарисуем сетку
            Gl.glBegin(Gl.GL_TRIANGLES);
            Gl.glColor3ub(255, 0, 0);
            Gl.glVertex3f(0, 1, 0);

            Gl.glColor3ub(0, 255, 0);
            Gl.glVertex3f(-1, 0, 0);

            Gl.glColor3ub(0, 0, 255);
            Gl.glVertex3f(1, 0, 0);
            Gl.glEnd();

            Gl.glPopMatrix();

            Gl.glFlush();

            AnT.Invalidate();
        }
        private void AnT_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mouseRotate = true; // Если нажата левая кнопка мыши
                label1.Text = "left";
            }
            if (e.Button == MouseButtons.Middle)
            {
                mouseMove = true; // Если нажата средняя кнопка мыши
                label1.Text = "midle";
            }
            myMouseYcoord = e.X; // Передаем в нашу глобальную переменную позицию мыши по Х
            myMouseXcoord = e.Y;
        }

        private void AnT_MouseUp(object sender, MouseEventArgs e)
        {
            mouseRotate = false;
            mouseMove = false;
        }

        private void AnT_MouseMove(object sender, MouseEventArgs e)
        {
            myMouseXcoordVar = e.Y;
            myMouseYcoordVar = e.X;
        }
        private void mouse_Events()
        {
            if (mouseRotate == true) // Если нажата левая кнопка мыши
            {
                AnT.Cursor = System.Windows.Forms.Cursors.SizeAll; //меняем указатель

                cam.Rotate_Position((float)(myMouseYcoordVar - myMouseYcoord), 0, 1, 0); // крутим камеру, в моем случае вид у нее от третьего лица

                rot_cam_X = rot_cam_X + (myMouseXcoordVar - myMouseXcoord);
                if ((rot_cam_X > -40) && (rot_cam_X < 40))
                    cam.upDown(((float)(myMouseXcoordVar - myMouseXcoord)) / 10);

                myMouseYcoord = myMouseYcoordVar;
                myMouseXcoord = myMouseXcoordVar;
            }
            else
            {
                if (mouseMove == true)
                {
                    AnT.Cursor = System.Windows.Forms.Cursors.SizeAll;

                    cam.Move_Camera((float)(myMouseXcoordVar - myMouseXcoord) / 50);
                    cam.Strafe(-((float)(myMouseYcoordVar - myMouseYcoord) / 50));

                    myMouseYcoord = myMouseYcoordVar;
                    myMouseXcoord = myMouseXcoordVar;

                }
                else
                {
                    AnT.Cursor = System.Windows.Forms.Cursors.Default; // возвращаем курсор
                };
            };
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            mouse_Events();
            cam.update();
            Draw();
        }
    }
}
