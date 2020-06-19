using System.Numerics;

namespace RayTracingInOneWeekend {

    public class XYRect : Hittable
    {
        public XYRect(in Vector2 min, in Vector2 max, float k, in Material material)
        {
            this.min = min;
            this.max = max;
            this.k = k;
            this.material = material;
        }

        public override bool hit(in Ray ray, float tMin, float tMax, ref HitRecord record)
        {
            float t = (k - ray.origin.Z) / ray.direction.Z;
            if (t <= tMin || t >= tMax)
                return false;

            Vector3 p = ray.at(t);
            if (p.X < min.X || p.X > max.X || p.Y < min.Y || p.Y > max.Y)
                return false;
            
            // uv
            record.t = t;
            record.point = p;
            record.setFaceNormal(ray, Vector3.UnitZ);
            record.material = material;
            
            return true;
        }

        private readonly Vector2 min;
        private readonly Vector2 max;
        private readonly float k;
        private readonly Material material;
    }

    public class XZRect : Hittable
    {
        public XZRect(in Vector2 min, in Vector2 max, float k, in Material material)
        {
            this.min = min;
            this.max = max;
            this.k = k;
            this.material = material;
        }

        public override bool hit(in Ray ray, float tMin, float tMax, ref HitRecord record)
        {
            float t = (k - ray.origin.Y) / ray.direction.Y;
            if (t <= tMin || t >= tMax)
                return false;

            Vector3 p = ray.at(t);
            if (p.X < min.X || p.X > max.X || p.Z < min.Y || p.Z > max.Y)
                return false;
            
            // uv
            record.t = t;
            record.point = p;
            record.setFaceNormal(ray, Vector3.UnitY);
            record.material = material;
            
            return true;
        }

        private readonly Vector2 min;
        private readonly Vector2 max;
        private readonly float k;
        private readonly Material material;
    }

    public class YZRect : Hittable
    {
        public YZRect(in Vector2 min, in Vector2 max, float k, in Material material)
        {
            this.min = min;
            this.max = max;
            this.k = k;
            this.material = material;
        }

        public override bool hit(in Ray ray, float tMin, float tMax, ref HitRecord record)
        {
            float t = (k - ray.origin.X) / ray.direction.X;
            if (t <= tMin || t >= tMax)
                return false;

            Vector3 p = ray.at(t);
            if (p.Y < min.X || p.Y > max.X || p.Z < min.Y || p.Z > max.Y )
                return false;
            
            // uv
            record.t = t;
            record.point = p;
            record.setFaceNormal(ray, Vector3.UnitX);
            record.material = material;
            
            return true;
        }

        private readonly Vector2 min;
        private readonly Vector2 max;
        private readonly float k;
        private readonly Material material;
    }

}
