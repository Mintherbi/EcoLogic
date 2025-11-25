

using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Runtime;

using System.IO;
using System.Diagnostics;
using System.Collections;


namespace PointCloudDiffusion.Component.Biomimicry
{
    public class Physarum : GH_Component
    {
        List<Agent> agents;
        double[,] trailField;
        Random rand = new Random();
        int width;
        int height;

        class Agent
        {
            public Point2d Position;
            public Vector2d Direction;
            public List<List<Point3d>> Segments;

            public Agent(Point2d pos, Vector2d dir)
            {
                Position = pos;
                Direction = dir;
                Segments = new List<List<Point3d>>();
                Segments.Add(new List<Point3d> { new Point3d(pos.X, pos.Y, 0) });
            }

            public void AddPoint(Point2d p, bool split)
            {
                if (split)
                    Segments.Add(new List<Point3d>());
                Segments[Segments.Count - 1].Add(new Point3d(p.X, p.Y, 0));
            }
        }

        public Physarum()
          : base("Physarum", "Physarum",
              "Physarum simulation on 3d surface",
              "EcoLogic", "1.BiomimicrySimulation")
        { }

        protected override void RegisterInputParams(GH_InputParamManager p)
        {
            p.AddSurfaceParameter("Surface", "S", "Target surface", GH_ParamAccess.item);
            p.AddIntegerParameter("AgentCount", "AC", "Number of agents", GH_ParamAccess.item, 500);
            p.AddNumberParameter("StepSize", "SS", "Movement step size", GH_ParamAccess.item, 1.0);
            p.AddNumberParameter("SensorAngle", "SA", "Sensor angle in degrees", GH_ParamAccess.item, 30.0);
            p.AddNumberParameter("SensorOffset", "SO", "Sensor offset distance", GH_ParamAccess.item, 3.0);
            p.AddIntegerParameter("NumSteps", "N", "Number of steps to run", GH_ParamAccess.item, 100);
            p.AddIntegerParameter("U", "U", "Grid width for mapping", GH_ParamAccess.item, 100);
            p.AddIntegerParameter("V", "V", "Grid height for mapping", GH_ParamAccess.item, 100);
            p.AddNumberParameter("PipeRadius", "PR", "Radius of tube mesh", GH_ParamAccess.item, 2.0);
            p.AddIntegerParameter("PipeCircleSegments", "PCS", "Segments around tube", GH_ParamAccess.item, 24);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager p)
        {
            p.AddCurveParameter("Curves2d", "C2d", "2D agent paths", GH_ParamAccess.list);
            p.AddCurveParameter("Curves3d", "C3d", "3D agent paths mapped to surface", GH_ParamAccess.list);
            p.AddMeshParameter("Tubes", "T", "Tube meshes along agent paths", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Surface Srf = null;
            int AgentCount = 50;
            double StepSize = 1.0;
            double SensorAngle = 30.0;
            double SensorOffset = 1.0;
            int NumSteps = 50;
            int U = 200;
            int V = 200;
            double PipeRadius = 0.1;
            int PipeCircleSegments = 8;

            DA.GetData(0, ref Srf);
            DA.GetData(1, ref AgentCount);
            DA.GetData(2, ref StepSize);
            DA.GetData(3, ref SensorAngle);
            DA.GetData(4, ref SensorOffset);
            DA.GetData(5, ref NumSteps);
            DA.GetData(6, ref U);
            DA.GetData(7, ref V);
            DA.GetData(8, ref PipeRadius);
            DA.GetData(9, ref PipeCircleSegments);

            width = U;
            height = V;

            if (agents == null)
            {
                agents = new List<Agent>();
                trailField = new double[width, height];

                for (int i = 0; i < AgentCount; i++)
                {
                    double x = rand.NextDouble() * width;
                    double y = rand.NextDouble() * height;

                    Vector2d dir = new Vector2d(rand.NextDouble() - 0.5, rand.NextDouble() - 0.5);
                    dir.Unitize();

                    agents.Add(new Agent(new Point2d(x, y), dir));
                }
            }

            // --- 2D Physarum simulation ---
            double sensorRad = SensorAngle * Math.PI / 180.0;
            for (int s = 0; s < NumSteps; s++)
            {
                foreach (var a in agents)
                {
                    Vector2d dir = a.Direction;
                    Vector2d leftDir = new Vector2d(dir.X, dir.Y); leftDir.Rotate(sensorRad);
                    Vector2d rightDir = new Vector2d(dir.X, dir.Y); rightDir.Rotate(-sensorRad);

                    Point2d forwardPos = a.Position + dir * SensorOffset;
                    Point2d leftPos = a.Position + leftDir * SensorOffset;
                    Point2d rightPos = a.Position + rightDir * SensorOffset;

                    double f = SampleTrail(forwardPos);
                    double l = SampleTrail(leftPos);
                    double r = SampleTrail(rightPos);

                    if (f > l && f > r) { }
                    else if (l > r) a.Direction.Rotate(sensorRad);
                    else if (r > l) a.Direction.Rotate(-sensorRad);
                    a.Direction.Unitize();

                    a.Position += a.Direction * StepSize;

                    // Bounce edges
                    if (a.Position.X < 0 || a.Position.X >= width) a.Direction.X *= -1;
                    if (a.Position.Y < 0 || a.Position.Y >= height) a.Direction.Y *= -1;
                    a.Position.X = Math.Max(0, Math.Min(width - 1e-6, a.Position.X));
                    a.Position.Y = Math.Max(0, Math.Min(height - 1e-6, a.Position.Y));

                    trailField[(int)a.Position.X, (int)a.Position.Y] += 1.0;
                    a.AddPoint(a.Position, false);
                }
            }

            // --- 2D polylines ---
            List<Polyline> polylines2D = new List<Polyline>();
            foreach (var a in agents)
            {
                foreach (var seg in a.Segments)
                    if (seg.Count > 1)
                        polylines2D.Add(new Polyline(seg));
            }

            DA.SetDataList(0, polylines2D);

            // --- Map to 3D surface ---
            List<PolylineCurve> polylines3D = new List<PolylineCurve>();
            if (Srf != null && polylines2D.Count > 0)
            {
                Brep brep = Srf.ToBrep();
                if (brep != null && brep.Faces.Count > 0)
                {
                    BrepFace face = brep.Faces[0];
                    Interval domU = face.Domain(0);
                    Interval domV = face.Domain(1);

                    double minX = double.MaxValue, maxX = double.MinValue;
                    double minY = double.MaxValue, maxY = double.MinValue;
                    foreach (var pl in polylines2D)
                        foreach (var pt in pl)
                        {
                            if (pt.X < minX) minX = pt.X;
                            if (pt.X > maxX) maxX = pt.X;
                            if (pt.Y < minY) minY = pt.Y;
                            if (pt.Y > maxY) maxY = pt.Y;
                        }
                    double rangeX = maxX - minX;
                    double rangeY = maxY - minY;

                    foreach (var pl in polylines2D)
                    {
                        Polyline current = new Polyline();
                        for (int i = 0; i < pl.Count; i++)
                        {
                            Point3d pt = pl[i];
                            double u = domU.T0 + ((pt.X - minX) / rangeX) * domU.Length;
                            double v = domV.T0 + ((pt.Y - minY) / rangeY) * domV.Length;

                            current.Add(face.PointAt(u, v));
                        }
                        if (current.Count > 1)
                            polylines3D.Add(new PolylineCurve(current));
                    }
                }
            }

            DA.SetDataList(1, polylines3D);

            // --- Tubes ---
            List<Mesh> meshes = new List<Mesh>();
            foreach (var crv in polylines3D)
            {
                if (crv == null || !crv.IsValid) continue;
                meshes.Add(CreateMeshTubeFromCurve(crv, PipeRadius, PipeCircleSegments, 12));
            }
            DA.SetDataList(2, meshes);
        }

        private double SampleTrail(Point2d p)
        {
            int x = ((int)p.X % width + width) % width;
            int y = ((int)p.Y % height + height) % height;
            return trailField[x, y];
        }

        private Mesh CreateMeshTubeFromCurve(Curve curve, double radius, int circleSegments, int samples)
        {
            if (curve == null || !curve.IsValid) return null;
            if (circleSegments < 3) circleSegments = 8;
            if (samples < 2) samples = 12;

            double t0 = curve.Domain.T0;
            double t1 = curve.Domain.T1;
            double[] tvals = new double[samples];
            for (int i = 0; i < samples; i++)
                tvals[i] = t0 + (t1 - t0) * i / (double)(samples - 1);

            List<Point3d> centers = new List<Point3d>();
            List<Vector3d> tangents = new List<Vector3d>();
            foreach (double t in tvals)
            {
                Point3d p = curve.PointAt(t);
                Vector3d tan = curve.TangentAt(t);
                tan.Unitize();
                centers.Add(p);
                tangents.Add(tan);
            }

            Mesh mesh = new Mesh();
            List<int[]> rings = new List<int[]>();
            for (int i = 0; i < centers.Count; i++)
            {
                Point3d c = centers[i];
                Vector3d t = tangents[i];

                Vector3d arbitrary = Vector3d.ZAxis;
                if (Math.Abs(arbitrary * t) > 0.9) arbitrary = Vector3d.XAxis;
                Vector3d xaxis = Vector3d.CrossProduct(arbitrary, t); xaxis.Unitize();
                Vector3d yaxis = Vector3d.CrossProduct(t, xaxis); yaxis.Unitize();

                int[] ring = new int[circleSegments];
                for (int s = 0; s < circleSegments; s++)
                {
                    double ang = 2.0 * Math.PI * s / circleSegments;
                    Point3d v = c + xaxis * (Math.Cos(ang) * radius) + yaxis * (Math.Sin(ang) * radius);
                    ring[s] = mesh.Vertices.Add(v);
                }
                rings.Add(ring);
            }

            for (int i = 0; i < rings.Count - 1; i++)
            {
                int[] r0 = rings[i];
                int[] r1 = rings[i + 1];
                for (int s = 0; s < circleSegments; s++)
                {
                    int sN = (s + 1) % circleSegments;
                    mesh.Faces.AddFace(r0[s], r0[sN], r1[sN], r1[s]);
                }
            }

            mesh.Normals.ComputeNormals();
            mesh.Compact();
            return mesh;
        }

        protected override System.Drawing.Bitmap Icon => null;
        public override Guid ComponentGuid => new Guid("95210E7B-3CFB-4DB3-AE64-A35EE4C0C06D");
    }
}

