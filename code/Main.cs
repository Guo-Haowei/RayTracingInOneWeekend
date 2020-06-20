using System;
using System.Threading.Tasks;
using System.Numerics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace RayTracingInOneWeekend
{
    class Program
    {
        static readonly float lightXMin = 213.0f;
        static readonly float lightXMax = 343.0f;
        static readonly float lightZMin = 227.0f;
        static readonly float lightZMax = 332.0f;
        static readonly float boxSize = 555.0f;
        static readonly float lightY = boxSize - 1.0f;

        static Vector3 rayColor(in Ray ray, in Vector3 background, in HittableList world, int depth)
        {
            if (depth <= 0)
                return Vector3.Zero;

            HitRecord record = new HitRecord();

            if (!(world.hit(ray, 0.001f, float.PositiveInfinity, ref record)))
                return background;

            Ray scattered = new Ray();
            // scattered.time = 0.0f;
            Material mat = record.material;
            Vector3 emitted = mat.emit(record.uv, record.point);
            float pdf = 0.0f;
            // Vector3 attenuation = new Vector3(0.0f);
            Vector3 albedo = new Vector3(0.0f);

            if (!mat.scatter(ray, record, ref albedo, ref scattered, ref pdf))
                return emitted;

            Vector3 onLight = new Vector3(
                Utility.RandomF(lightXMin, lightXMax),
                lightY,
                Utility.RandomF(lightZMin, lightZMax)
            );

            Vector3 toLight = onLight - record.point;
            float distanceSqrToLight = Vector3.Dot(toLight, toLight);
            toLight = Vector3.Normalize(toLight);

            if (Vector3.Dot(toLight, record.normal) < 0.0f)
                return emitted;

            float lightArea = (lightXMax - lightXMin) * (lightZMax - lightZMin);
            float lightCosine = Math.Abs(toLight.Y);
            if (lightCosine < 0.00001f)
                return emitted;

            pdf = distanceSqrToLight / (lightCosine * lightArea);
            scattered.origin = record.point;
            scattered.direction = toLight;

            float scatteredPdf = mat.scatterPdf(ray, record, scattered);
            return emitted + albedo * scatteredPdf * rayColor(scattered, background, world, depth - 1) / pdf;
        }

        static HittableList createScene(out Camera camera, float aspect)
        {
            HittableList world = new HittableList();

            var red = new Lambertian(new SolidColor(0.65f, 0.05f, 0.05f));
            var green = new Lambertian(new SolidColor(0.12f, 0.45f, 0.15f));
            var white = new Lambertian(new SolidColor(0.73f));
            var light = new DiffuseLight(new SolidColor(15.0f));

            float s = boxSize;
            world.add(new YZRect(Vector3.Zero, s * Vector3.One, s, green));
            world.add(new YZRect(Vector3.Zero, s * Vector3.One, 0.0f, red));
            world.add(new XZRect(Vector3.Zero, s * Vector3.One, s, white));
            world.add(new XZRect(Vector3.Zero, s * Vector3.One, 0.0f, white));
            world.add(new XYRect(Vector3.Zero, s * Vector3.One, s, white));
            // add light
            {
                Vector3 lmin = new Vector3(lightXMin, 0.0f, lightZMin);
                Vector3 lmax = new Vector3(lightXMax, 0.0f, lightZMax);
                world.add(new XZRect(lmin, lmax, lightY, light));
            }
            // add sphere
            {
                Vector3 min = new Vector3(130.0f, 0.0f, 65.0f);
                Vector3 max = new Vector3(295.0f, 165.0f, 230.0f);
                Vector3 center = 0.5f * (min + max);
                float radius = 0.5f * (max - min).X;
                // world.add(new Box(new Vector3(130.0f, 0.0f, 65.0f), new Vector3(295.0f, 165.0f, 230.0f), white));
                world.add(new Sphere(center, radius, white));
            }
            // add box
            {
                world.add(new Box(new Vector3(265.0f, 0.0f, 295.0f), new Vector3(430.0f, 330.0f, 460.0f), white));
            }

            Vector3 lookFrom = new Vector3(278.0f, 278.0f, -800.0f);
            Vector3 lookAt = new Vector3(278.0f, 278.0f, 0.0f);
            float focusDistance = 10.0f;
            float aperture = 0.0f;
            float fov = 40.0f;
            camera = new Camera(
                lookFrom,
                lookAt,
                Vector3.UnitY,
                fov,
                aspect,
                aperture,
                focusDistance,
                0.0f,
                1.0f);

            return world;
        }

        static void Main()
        {
            const int imageWidth = 384;
            const int imageHeight = imageWidth;
            // const int imageHeight = 216;
            const float aspectRatio = (float)imageWidth / imageHeight;
            const int samplesPerPixel = 10;
            // const int samplesPerPixel = 100;
            const int maxDepth = 50;

            const int component = 3;
            const int stride = component * imageWidth;

            byte[] imageBuffer = new byte[stride * imageHeight];

            Camera camera;

            HittableList world = createScene(out camera, aspectRatio);

            DateTime start = DateTime.Now;
            Console.WriteLine("Start at: {0}", start.ToString("F"));

            Parallel.For(0, imageWidth * imageHeight,
                index => {
                    int i = index % imageWidth;
                    int j = imageHeight - (index / imageWidth + 1);

                    Vector3 color = Vector3.Zero;
                    for (int s = 0; s < samplesPerPixel; ++s)
                    {
                        float u = (i + Utility.RandomF()) / (imageWidth - 1);
                        float v = (j + Utility.RandomF()) / (imageHeight - 1);
                        Ray ray = camera.getRay(u, v);
                        color += rayColor(ray, Vector3.Zero, world, maxDepth);
                    }

                    // devide the color total by the number of samples and gamma-correct for gamma = 2.0
                    float scale = 1.0f / samplesPerPixel;
                    float r = color.X;
                    float g = color.Y;
                    float b = color.Z;
                    r = (float)Math.Sqrt(scale * r);
                    g = (float)Math.Sqrt(scale * g);
                    b = (float)Math.Sqrt(scale * b);
                    imageBuffer[3 * index + 0] = (byte)(255.999f * Utility.Clamp(b, 0.0f, 1.0f));
                    imageBuffer[3 * index + 1] = (byte)(255.999f * Utility.Clamp(g, 0.0f, 1.0f));
                    imageBuffer[3 * index + 2] = (byte)(255.999f * Utility.Clamp(r, 0.0f, 1.0f));
                }
            );

            Bitmap bitmap = new Bitmap(imageWidth, imageHeight, stride, PixelFormat.Format24bppRgb, Marshal.UnsafeAddrOfPinnedArrayElement(imageBuffer, 0));
            bitmap.Save("../image.png", ImageFormat.Png);
            bitmap.Dispose();

            DateTime end = DateTime.Now;
            Console.WriteLine("End at: {0}", end.ToString("F"));
            TimeSpan deltaTime = end - start;
            Console.WriteLine("Took: {0} hours {1} minutes {2} seconds", deltaTime.Hours, deltaTime.Minutes, deltaTime.Seconds + Math.Round(deltaTime.Milliseconds / 1000.0, 3));
        }
    }
}
