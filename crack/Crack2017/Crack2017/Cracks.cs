using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Numerics;
using System.Media;

namespace Cracks
{
    class CrackWorld
    {
        public struct Point3D
        {
            public float X, Y, Z;
            //public int Xi, Yi, Zi;
            public Point3D(float x1, float y1, float z1)
            {
                X = x1;
                Y = y1;
                Z = z1;
            }
            public Point3D(int x1, int y1, int z1)
            {
                X = x1;
                Y = y1;
                Z = z1;
            }

        }
        private Point[] m_SurfaceTensionForces; //силы поверхностного натяжения

        private const double ForceEpsilon = float.Epsilon;

        public PointF[][] BrideSurfaceCurves { get; private set; } //мостики
        public PointF[][] ParticlesPositions { get; private set; }// местоположение частиц
        public Point3D[][][] particles { get; private set; }
        public double ParticleRadius { get; private set; } //радиус частиц

        public int criticalLength;
        private double[][][] velocityX, velocityY, velocityZ;
        private double deltaT = 1.0f;
        private float[][][] x, y, z;
        public int n, m, u;
        BigInteger[] polynomForceCoeff;
        private double end;
        public double mass = 16.0f;
        public double remTime = 0.1f;
        public double alpha;


        public CrackWorld(float[][][] xPositions, float[][][] yPositions, float[][][] zPositions, double particleRadius, int criticalLength, BigInteger[] poly, double end)
        {
            alpha = mass * remTime;
            polynomForceCoeff = poly;
            this.end = end;
            this.criticalLength = criticalLength;
            ParticleRadius = particleRadius;
            n = xPositions.Length;
            m = yPositions[0].Length;
            u = zPositions[0][0].Length;

            velocityX = new double[n][][];
            velocityY = new double[n][][];
            velocityZ = new double[n][][];

            ParticlesPositions = new PointF[n][];//----
            particles = new Point3D[n][][];

            for (int i = 0; i < n; i++)
            {
                ParticlesPositions[i] = new PointF[m];//---

                particles[i] = new Point3D[m][];

                velocityX[i] = new double[m][];
                velocityY[i] = new double[m][];
                velocityZ[i] = new double[m][];

            }

            for (int i = 0; i < n; i++)
                for (int j = 0; j < m; j++)
                {//TODO: zzz
                    //ParticlesPositions[i][j] = new PointF(xPositions[i][j], yPositions[i][j]);//---
                    particles[i][j] = new Point3D[u];

                    velocityX[i][j] = new double[u];
                    velocityY[i][j] = new double[u];
                    velocityZ[i][j] = new double[u];

                    for (int k = 0; k < u; k++)
                    {
                        particles[i][j][k] = new Point3D(xPositions[i][j][k], yPositions[i][j][k], zPositions[i][j][k]);
                    }
                }

            x = xPositions;
            y = yPositions;
            z = zPositions;

            m_SurfaceTensionForces = new Point[(n - 2) * (m - 2)];
            BrideSurfaceCurves = new PointF[n * m * n][];

        }

        public void Update()
        {
            for (int i = 1; i < n - 1; i++)
                for (int j = 1; j < m; j++)
                {
                    for (int e = 1; e < u - 1; e++)
                    {
                        //x[i][j] = ParticlesPositions[i][j].X;
                        //y[i][j] = ParticlesPositions[i][j].Y;
                        x[i][j][e] = particles[i][j][e].X;
                        y[i][j][e] = particles[i][j][e].Y;
                        z[i][j][e] = particles[i][j][e].Z;

                        Point3D[] index = new Point3D[6];
                        index[0] = j != m - 1 ? new Point3D(i, j + 1, e) : new Point3D(i, j, e); //up
                        index[1] = new Point3D(i, j - 1, e); //down
                        index[2] = new Point3D(i - 1, j, e); //left
                        index[3] = new Point3D(i + 1, j, e); //right
                        index[4] = new Point3D(i, j, e - 1); //front
                        index[5] = new Point3D(i, j, e + 1); //back

                        Boolean change = true;

                        for (int k = 0; k < 6; k++)
                        {
                            change = change && (Math.Abs(x[i][j][e] - x[(int)index[k].X][(int)index[k].Y][(int)index[k].Z]) >= 2 * ParticleRadius + 0.01
                                             || Math.Abs(y[i][j][e] - y[(int)index[k].X][(int)index[k].Y][(int)index[k].Z]) >= 2 * ParticleRadius + 0.01
                                             || Math.Abs(z[i][j][e] - z[(int)index[k].X][(int)index[k].Y][(int)index[k].Z]) >= 2 * ParticleRadius + 0.01);
                        }
                        bool xb = x[i][j][e] >= x[0][0][0] + 2 * ParticleRadius + 0.01 && x[i][j][e] <= x[n - 1][m - 1][e - 1] - 2 * ParticleRadius - 0.01;
                        bool yb = y[i][j][e] >= y[0][0][0] + 2 * ParticleRadius + 0.01 && y[i][j][e] <= y[n - 1][m - 1][e - 1] - 2 * ParticleRadius - 0.01;
                        bool zb = Math.Abs(z[i][j][e]) >= Math.Abs(z[0][0][0] + 2 * ParticleRadius + 0.01) && (z[i][j][e]) <= (z[n - 1][m - 1][e - 1] - 2 * ParticleRadius - 0.01);
                        if (xb
                            && yb
                            && zb
                            && change)
                        {

                            Point3D force = surfaceTensionForces(i, j, e);

                            velocityX[i][j][e] += deltaT * force.X / mass;
                            velocityY[i][j][e] += deltaT * force.Y / mass;
                            velocityZ[i][j][e] += deltaT * force.Z / mass;

                            x[i][j][e] = (int)Math.Ceiling(x[i][j][e] + velocityX[i][j][e] * deltaT);
                            y[i][j][e] = (int)Math.Ceiling(y[i][j][e] + velocityY[i][j][e] * deltaT);
                            z[i][j][e] = (int)Math.Ceiling(z[i][j][e] + velocityZ[i][j][e] * deltaT);

                            particles[i][j][e].X = x[i][j][e];
                            particles[i][j][e].Y = y[i][j][e];
                            particles[i][j][e].Z = z[i][j][e];

                        }
                    }
                }

            int count = 0;
            for (int i = 1; i < n - 1; i++)
                for (int j = 1; j < m - 1; j++)
                    for (int k = 0; k < 4; k++)
                    {
                        BrideSurfaceCurves[count] = BuildBridgeSurface(i, j, 0, k);
                        count++;
                    }
        }
        private Point3D surfaceTensionForces(int icur, int jcur, int ecur)
        {
            Point3D force = new Point3D();
            Point3D[] index = new Point3D[6];
            index[0] = new Point3D(icur - 1, jcur, ecur); //up
            index[1] = new Point3D(icur + 1, jcur, ecur); //down
            index[2] = new Point3D(icur, jcur - 1, ecur); //left
            index[3] = new Point3D(icur, jcur + 1, ecur); //right
            index[4] = new Point3D(icur, jcur, ecur + 1); //front
            index[5] = new Point3D(icur, jcur, ecur - 1); //back

            double forceX = 0.0f, forceY = 0.0f, forceZ = 0.0f;

            for (int i = 0; i < 4; i++)
            {
                double diffX = (x[(int)index[i].X][(int)index[i].Y][(int)index[i].Z] - x[icur][jcur][ecur]) * Math.Pow(10, -7);
                double diffY = (y[(int)index[i].X][(int)index[i].Y][(int)index[i].Z] - y[icur][jcur][ecur]) * Math.Pow(10, -7);
                double diffZ = (z[(int)index[i].X][(int)index[i].Y][(int)index[i].Z] - z[icur][jcur][ecur]) * Math.Pow(10, -7);
                double dist = distRad(diffX, diffY, diffZ);

                if (dist * Math.Pow(10, 7) - criticalLength < 0.0f)
                {
                    double polynomFX = 0, polynomFY = 0, polynomFZ = 0;
                    int N = polynomForceCoeff.Length;
                    int pow;
                    double curr;

                    for (int j = 0; j < N; j++)
                    {
                        pow = (int)BigInteger.Log10(BigInteger.Abs(polynomForceCoeff[j]));
                        curr = (double)BigInteger.Divide(polynomForceCoeff[j], (BigInteger)Math.Pow(10, pow - 3));
                        Console.WriteLine(curr + " " + polynomForceCoeff[j]);
                        polynomFX += curr * Math.Pow(10, pow - 3) * Math.Pow(Math.Abs(diffX), N - j);
                        polynomFX += curr * Math.Pow(10, pow - 3) * Math.Pow(Math.Abs(diffY), N - j);
                        polynomFX += curr * Math.Pow(10, pow - 3) * Math.Pow(Math.Abs(diffZ), N - j);
                    }
                    polynomFX += end;
                    polynomFY += end;
                    polynomFZ += end;

                    forceX += (diffX * polynomFX / dist);
                    forceY += (diffY * polynomFY / dist);
                    forceZ += (diffZ * polynomFZ / dist);

                }
                else
                {
                    forceX = 0.0f;
                    forceY = 0.0f;
                    forceZ = 0.0f;
                }
            }
            force.X = (int)(Math.Ceiling(forceX - alpha * velocityX[icur][jcur][ecur]));
            force.Y = (int)(Math.Ceiling(forceY - alpha * velocityY[icur][jcur][ecur]));
            force.Z = (int)(Math.Ceiling(forceZ - alpha * velocityZ[icur][jcur][ecur]));

            return force;
        }

        private PointF[] BuildBridgeSurface(int icur, int jcur, int ecur, int k)
        {
            Point[] index = new Point[4];
            index[0] = new Point(icur - 1, jcur); //up
            index[1] = new Point(icur + 1, jcur); //down
            index[2] = new Point(icur, jcur - 1); //left
            index[3] = new Point(icur, jcur + 1); //right

            PointF fstPosition, sndPosition;

            int i = index[k].X, j = index[k].Y;

            sndPosition = new PointF(x[icur][jcur][ecur], y[icur][jcur][ecur]);// здесь ecur тоже добавлен только, чтобы избежать ошибки
            fstPosition = new PointF(x[i][j][0], y[i][j][0]); // так как эта вся функция больше не требуется, индекс написан "0". результаты совсем неизвестны. Сделано только для того, чтобы прошло компиляцию


            var surfLengthX = Math.Abs(sndPosition.X - fstPosition.X);
            var surfLengthY = Math.Abs(sndPosition.Y - fstPosition.Y);

            var surfPoints = new PointF[2];
            int p = icur * m + jcur + 1, q = i * m + j + 1;

            if ((distRad(surfLengthY, surfLengthX, 0) - criticalLength) >= 0.0f)
                return null;
            else
            {

                surfPoints[0] = fstPosition;
                surfPoints[1] = sndPosition;

                return surfPoints;
            }


            /*
            var surfLengthX = Math.Abs(sndPosition.X - fstPosition.X);
            var surfLengthY = Math.Abs(sndPosition.Y - fstPosition.Y);

            if (Math.Abs(distRad(surfLengthY, surfLengthX) - criticalLength) <= 10.0f)
                return null;



            if (surfLengthX != 0)
            {
                var k = (sndPosition.Y - fstPosition.Y) / (sndPosition.X - fstPosition.X);
                var b = sndPosition.Y - k * sndPosition.X;

                var surfPoints = new Point[(int)distRad(surfLengthY, surfLengthX)];
                var cy = 0;
                var cx = sndPosition.X > fstPosition.X ? fstPosition.X : sndPosition.X;
                for (var i = 0; i < surfPoints.Length; i++)
                {
                    cy = (int)k * cx + b;
                    surfPoints[i] = new Point(cx, cy);
                    cx++;
                }

                return surfPoints;
            }
            else
            {
                var surfPoints = new Point[(int)distRad(surfLengthY, surfLengthX)];
                var cy = sndPosition.Y > fstPosition.Y ? fstPosition.Y : sndPosition.Y;
                var cx = fstPosition.X;
                for (var i = 0; i < surfPoints.Length; i++)
                {
                    surfPoints[i] = new Point(cx, cy);
                    cy++;
                }

                return surfPoints;
            }
            */
            /*
            var surfLengthX = (sndPosition.X - fstPosition.X);
            var surfLengthY = Math.Abs(sndPosition.Y - fstPosition.Y);
            
            if (distRad(surfLengthY, surfLengthX) >= criticalLength)
                return null;
            
            var cx = fstPosition.X;
            var cy = fstPosition.Y - ParticleRadius;


            if (surfLengthY == 0)
            {
                var x0 = fstPosition.X + surfLengthX / 2.0;
                var y0 = surfLengthX;
                var ac = (cy - y0) / Math.Pow(cx - x0, 2.0);

                var surfPoints = new Point[surfLengthX];
                for (var i = 0; i < surfPoints.Length; i++)
                {
                    cy = (int)(Math.Pow(cx - x0, 2.0) * ac + y0);
                    surfPoints[i] = new Point(cx, cy);
                    cx++;
                }

                return surfPoints;
            }

            if (surfLengthX == 0)
            {
                var x0 = fstPosition.X + surfLengthX / 2.0;
                var y0 = surfLengthX;
                var ac = (cy - y0) / Math.Pow(cx - x0, 2.0);

                var surfPoints = new Point[surfLengthX];
                for (var i = 0; i < surfPoints.Length; i++)
                {
                    cy = (int)(Math.Pow(cx - x0, 2.0) * ac + y0);
                    surfPoints[i] = new Point(cx, cy);
                    cx++;
                }

                return surfPoints;
            }
            else return null;
            */
            /*
            var surfPoints = new Point[4];

            surfPoints[0].X = fstPosition.X;
            surfPoints[1].X = fstPosition.X + Math.Abs((sndPosition.X - fstPosition.X) / 2);
            surfPoints[3].X = sndPosition.X;
            surfPoints[0].Y = fstPosition.Y - ParticleRadius;
            surfPoints[1].Y = fstPosition.Y - 20;
            surfPoints[3].Y = sndPosition.Y - ParticleRadius;

            surfPoints[2].X = surfPoints[1].X;
            surfPoints[2].Y = surfPoints[1].Y;



            return surfPoints;
*/
        }

        public double distRad(double A, double B, double C)
        {
            return Math.Pow(A * A + B * B + C * C, 0.5);
        }

        private void CalculateForces()
        {
            throw new NotImplementedException();
        }

        public bool IsStatic()
        {
            /*x[1] = ParticlesPositions[1].X;
            y[1] = ParticlesPositions[1].Y;
           return Math.Abs(distRad(x[1] - x[0], y[1] - y[0]) - 2 * ParticleRadius) <= 10.0f
                || Math.Abs(distRad(x[2] - x[1], y[2] - y[1]) - 2 * ParticleRadius) <= 10.0f
                || Math.Abs(distRad(x[1] - x[3], y[1] - y[3]) - 2 * ParticleRadius) <= 10.0f
                || Math.Abs(distRad(x[4] - x[1], y[4] - y[1]) - 2 * ParticleRadius) <= 10.0f
                || Math.Abs(y[1] - y[4]) <= 10.0f || Math.Abs(y[1] - y[3]) <= 10.0f || Math.Abs(x[1] - x[0]) <= 10.0f
                  || Math.Abs(x[1] - x[2]) <= 10.0f;*/
            // return m_SurfaceTensionForces.Any(f => Math.Abs(f) <= ForceEpsilon);
            return false;
        }
    }
}
