using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Numerics;


// для работы с библиотекой OpenGL 
using Tao.OpenGl;
// для работы с библиотекой FreeGLUT 
using Tao.FreeGlut;
// для работы с элементом управления SimpleOpenGLControl 
using Tao.Platform.Windows;
using Engine;
using Cracks;
using System.IO;
using System.Drawing.Imaging;


namespace Crack2017
{
    public partial class Form1 : Form
    {

        double angle = 0;
        double radius = 2;
        bool f = true, mouseRotate = false, mouseMove = false;
        int myMouseYcoord, myMouseXcoord, myMouseXcoordVar, myMouseYcoordVar, rot_cam_X;
        float offsetX = 0.3f, offsetY = 0.4f, offsetZ = 0.0f;
        Camera cam = new Camera();
        CrackWorld mWorld;

        public Form1()
        {

            InitializeComponent();
            AnT.InitializeContexts();
            InizializatePos();
        }

        private void Form1_Load(object sender, EventArgs e)
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
            Gl.glEnable(Gl.GL_COLOR_MATERIAL); // позволяет менять цвета!!!!!!!!!!!!!!
            Gl.glEnable(Gl.GL_LIGHTING);
            Gl.glEnable(Gl.GL_LIGHT0);

            //cam.Position_Camera(-3.62f, 0.6f, -4.98f, -3.65f, 0.52f, -6.98f, 0, 1, 0);
            //cam.Position_Camera(-2.2f, 0.6f, 8.08f, -2.23f, 0.51f, 6.08f, 0, 1, 0);// Вот тут в инициализации
            cam.Position_Camera(5.23f, 0.299f, 9.62f, 5.16f, 0.51f, 7.62f, 0, 1, 0);
            // укажем начальную позицию камеры, взгляда и вертикального вектора.
            cam.update();
            Draw2D();
            timer1.Start();
        }


        private void Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Visual_Click(object sender, EventArgs e)
        {
            DrawCube();//Запуск обычной отрисовки
        }
        //Функция отрисовки без анимации
        private void DrawCube()
        {
            int delta = 3;
            //double angle = 10.0;
            //for (int e = 0; e < 5; e++)
            // {
            // очистка буфера цвета и буфера глубины 
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            Gl.glClearColor(255, 255, 255, 1);
            // очищение текущей матрицы 
            Gl.glLoadIdentity();

            cam.Look(); // Обновляем взгляд камеры
            int n = 5;
            double x, y, z, width = AnT.Width, height = AnT.Height;
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    for (int k = 0; k < n; k++)
                    {
                        x = -width / 100 + i * delta;
                        y = -height / 80 + j * delta;
                        z = -20 - k * delta;
                        // помещаем состояние матрицы в стек матриц, дальнейшие 
                        //трансформации затронут только визуализацию объекта 
                        Gl.glPushMatrix();
                        Gl.glTranslated(x, y, z);

                        // поворот по установленной оси 
                        //Gl.glRotated(angle, 0, 1, 1);



                        //рисуем "мосты" из цилиндров
                        if (k != n && k != 0) //по z
                            Glut.glutSolidCylinder(0.2, delta, 32, 32);
                        if (j != n && i != n - 1) //по x
                        {
                            Gl.glRotated(90, 0, 1, 0);
                            Glut.glutSolidCylinder(0.2, delta, 32, 32);
                        }
                        if (i != n && j != 0) //по y
                        {
                            Gl.glRotated(90, 1, 0, 0);
                            Glut.glutSolidCylinder(0.2, delta, 32, 32);
                        }

                        // рисуем сферу с помощью библиотеки FreeGLUT 
                        Glut.glutSolidSphere(0.5, 32, 32);

                        Gl.glPopMatrix(); // возвращаемся к старой системе координат
                    }

            // завершаем рисование
            Gl.glFlush();

            // обновляем элемент AnT 
            AnT.Invalidate();
            //  }
        }
        //Функция отрисовки с анимацией. Отличается от предыдущей наличием переменного(переменных) значения. Эта функция вызывается каждое срабатывание таймера
        private void Draw()
        {

            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);//эти две строчки не трогаем
            Gl.glClearColor(255, 255, 255, 1);

            // очищение текущей матрицы 
            Gl.glLoadIdentity();
            //DrawGrid(30, 1);
            cam.Look(); // Обновляем взгляд камеры
            int nn = 5;
            double x, y, z, width = AnT.Width, height = AnT.Height;



            for (int i = 0; i < nn; i++)
                for (int j = 0; j < nn; j++)
                    for (int k = 0; k < nn; k++)
                    {
                        x = -width / 100 + i * radius;
                        y = -height / 80 + j * radius;
                        z = -20 - k * radius;

                        // помещаем состояние матрицы в стек матриц, дальнейшие 
                        //трансформации затронут только визуализацию объекта 
                        Gl.glPushMatrix();
                        Gl.glTranslated(x, y, z);

                        // поворот по установленной оси 
                        // Gl.glRotated(angle, 0, 1, 0);

                        //рисуем "мосты" из цилиндров
                        if (k != nn && k != 0) //по z
                            Glut.glutSolidCylinder(0.2, radius, 32, 32);
                        if (j != nn && i != nn - 1) //по x
                        {
                            Gl.glRotated(90, 0, 1, 0);
                            Glut.glutSolidCylinder(0.2, radius, 32, 32);
                        }
                        if (i != nn && j != 0) //по y
                        {
                            Gl.glRotated(90, 1, 0, 0);
                            Glut.glutSolidCylinder(0.2, radius, 32, 32);
                        }

                        // рисуем сферу с помощью библиотеки FreeGLUT 
                        Glut.glutSolidSphere(0.5, 32, 32);

                        Gl.glPopMatrix(); // возвращаемся к старой системе координат

                    }

            //анимация
            /*if (f)
                delta += 0.1;
            else
                delta -= 0.1;
            if (delta > 4)
                f = false;
            if (delta < 1)
                f = true;
            if (angle / 360 > 1)
            {
                angle = 0;
            }*/

            // завершаем рисование
            Gl.glFlush();

            // обновляем элемент AnT 
            AnT.Invalidate();
            //  
        }
        private void Draw2D()
        {

            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);//эти две строчки не трогаем
            Gl.glClearColor(255, 255, 255, 1); // цвет фона

            // очищение текущей матрицы 
            Gl.glLoadIdentity();
            //DrawGrid(30, 1);
            cam.Look(); // Обновляем взгляд камеры
            float x, y, z, width = AnT.Width, height = AnT.Height, x2, y2;

            for (int i = 0; i < mWorld.n; i++)
            {
                for (int j = 0; j < mWorld.m; j++) 
                {
                    for (int k = 1; k < mWorld.u; k++) // не рисуем переднюю стенку
                    {
                        var particle = mWorld.particles[i][j][k];

                        x = particle.X;
                        y = particle.Y;
                        z = particle.Z;
                        if (i == 0 && j == 0)
                            Console.WriteLine(x.ToString() + "  " + y.ToString() + "  " + z.ToString());
                        // помещаем состояние матрицы в стек матриц, дальнейшие 
                        //трансформации затронут только визуализацию объекта 

                        Gl.glPushMatrix();
                        Gl.glTranslated(x, y, z);


                        if (j + 1 < mWorld.m) // есть ли верхняя и нижняя связь
                        {
                            Gl.glColor3f(0.0f, 1.0f, 0.0f);
                            int a = j + 1;
                            var particle_up = mWorld.particles[i][a][k];
                            if (mWorld.distRad(Math.Abs(particle_up.Y - y), Math.Abs(particle_up.X - x), Math.Abs(particle_up.Z - z)) - mWorld.criticalLength < 0.0f)
                                renderCylinder(x, y, z, particle_up.X, particle_up.Y, particle.Z, 0.2f);
                        }
                        if (i + 1 < mWorld.n)  // есть ли правая левая
                        {
                            Gl.glColor3f(0.0f, 0.0f, 1.0f);
                            int a = i + 1;
                            var particle_right = mWorld.particles[a][j][k];
                            if (mWorld.distRad(Math.Abs(particle_right.Y - y), Math.Abs(particle_right.X - x), Math.Abs(particle_right.Z - z)) - mWorld.criticalLength < 0.0f)
                                renderCylinder(x, y, z, particle_right.X, particle_right.Y, particle.Z, 0.2f);
                            //renderCylinder(x, y, z, particle_right.X, particle_right.Y, z, 0.2f);
                        }
                        if (k + 1 < mWorld.u) // есть ли ближняя и дальняя связь
                        {
                            Gl.glColor3f(0.5f, 0.5f, 0.5f);
                            int a = k + 1;
                            var particle_near = mWorld.particles[i][j][a];
                            if (mWorld.distRad(Math.Abs(particle_near.Y - y), Math.Abs(particle_near.X - x), Math.Abs(particle_near.Z - z)) - mWorld.criticalLength <= 0.0f)
                                renderCylinder(x, y, z, particle_near.X, particle_near.Y, particle.Z, 0.2f);

                        }

                        Gl.glColor3f(1.0f, 0.0f, 0.0f); // красные шарики
                        Glut.glutSolidSphere(0.5, 32, 32);

                        Gl.glPopMatrix(); // возвращаемся к старой системе координат
                    }
                }
            }
            Gl.glFlush();

            // обновляем элемент AnT 
            AnT.Invalidate();
            //  
        }
        private void InizializatePos()
        {
            BigInteger[] numArray;
            double critical;
            double end;

            using (StreamReader reader = new StreamReader("forces.txt", Encoding.Default))
            {
                string buff = reader.ReadLine();
                buff = buff.Replace('.', ',');

                string[] array2 = buff.Split(' ');
                string[] array = new string[array2.Length - 1];
                Array.Copy(array2, array, array2.Length - 1);//Костыль, т.к. последний элемент парсится как " "

                numArray = new BigInteger[array.Length - 2];
                critical = Convert.ToDouble(array[0]) * Math.Pow(10, 7) / 3;
                end = Convert.ToDouble(array[array.Length - 1]);

                for (int i = 0; i < array.Length - 2; i++)
                {
                    numArray[i] = BigInteger.Parse(array[i + 1].Split(',')[0]);
                }
            }


            int n = 5;
            int m = 7;
            int u = 5;
            float[][][] xPositions = new float[n][][];
            float[][][] yPositions = new float[n][][];
            float[][][] zPositions = new float[n][][];

            float width = AnT.Width, height = AnT.Height;
            for (int i = 0; i < n; i++)
            {
                xPositions[i] = new float[m][];
                yPositions[i] = new float[m][];
                zPositions[i] = new float[m][];
            }
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    xPositions[i][j] = new float[u];
                    yPositions[i][j] = new float[u];
                    zPositions[i][j] = new float[u];
                }
            }
            float len = 2.0f;
            for (int i = 0; i < n; i++)
                for (int j = 0; j < m; j++)
                {
                    for (int k = 0; k < u; k++)
                    {
                        if (i == 100 && j == 90 && k == 2000)
                        {
                            xPositions[i][j][k] = -width / 100 + i * len - offsetX;//вместо 0 писать сдвиг
                            yPositions[i][j][k] = -height / 80 + j * len - offsetY;
                            zPositions[i][j][k] = -20 - k * len - offsetZ;
                        }
                        else
                        {
                            xPositions[i][j][k] = -width / 100 + i * len;
                            yPositions[i][j][k] = -height / 80 + j * len;
                            zPositions[i][j][k] = -20 - k * len;
                        }
                    }
                }

            var pRadius = 0.5;
            var criticalLength = (int)critical;
            mWorld = new CrackWorld(xPositions, yPositions, zPositions, pRadius, criticalLength / 8, numArray, end);
        }
        private void button2_Click(object sender, EventArgs e)
        {
            //Запуск таймера
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label1.Text = cam.getPosX().ToString();
            label2.Text = cam.getPosY().ToString();
            label3.Text = cam.getPosZ().ToString();
            label4.Text = cam.getViewX().ToString();
            label5.Text = cam.getViewY().ToString();
            label6.Text = cam.getViewZ().ToString();
            mWorld.Update();
            mouse_Events();
            cam.update();
            Draw2D();

        }
        private void renderCylinder(float x1, float y1, float z1, float x2, float y2, float z2, float radius)
        {
            Glu.GLUquadric quadric = Glu.gluNewQuadric();
            Glu.gluQuadricNormals(quadric, Glu.GLU_SMOOTH);
            float vx = x2 - x1;
            float vy = y2 - y1;
            float vz = z2 - z1;

            //handle the degenerate case of z1 == z2 with an approximation
            if (vz > -0.000001f && vz < 0.000001f)
                if (vz >= 0.0)
                    vz = 0.000001f;
                else
                    vz = -0.000001f;

            float v = (float)Math.Sqrt(vx * vx + vy * vy + vz * vz);
            float ax = (float)Math.Acos(vz / v) * 57.2957795f;
            if (vz < 0.0)
                ax = -ax;
            float rx = -vy * vz;
            float ry = vx * vz;
            Gl.glPushMatrix();

            //draw the cylinder body
            //Gl.glTranslatef(x1, y1, z1);
            //Gl.glRotatef(ax, rx, ry, 0.0f);
            Gl.glRotatef(ax, rx, ry, 0.0f);
            Glu.gluQuadricOrientation(quadric, Glu.GLU_OUTSIDE);
            // Glut.glutSolidCylinder(quadric, radius, radius, v, subdivisions, 1);
            Glu.gluCylinder(quadric, radius, radius, v, 32, 1);
            //Glut.glutSolidCylinder(radius, v, 32, 32);

            //draw the first cap
            /* Gl.gluQuadricOrientation(quadric, GLU_INSIDE);
             Gl.gluDisk(quadric, 0.0, radius, subdivisions, 1);
             Gl.glTranslatef(0, 0, v);

             //draw the second cap
             Gl.gluQuadricOrientation(quadric, GLU_OUTSIDE);
             Gl.gluDisk(quadric, 0.0, radius, subdivisions, 1);*/
            Gl.glPopMatrix();
            Glu.gluDeleteQuadric(quadric);
        }

        private void AnT_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                mouseRotate = true; // Если нажата левая кнопка мыши


            if (e.Button == MouseButtons.Right)
                mouseMove = true; // Если нажата средняя кнопка мыши

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

    }
}
