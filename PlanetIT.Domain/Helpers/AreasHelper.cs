using Microsoft.AspNetCore.Http;
using PlanetIT.Domain.Models.Data;
using PlanetIT.Domain.Models.Request;
using PlanetIT.Domain.Models.Response;
using PlanetIT.Domain.Response;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PlanetIT.Domain.Helpers
{
    public static class AreasHelper
    {
        // Проверяет находится ли точка на линии
        public static bool IsPointOnLine(Vector linePoint1, Vector linePoint2, Vector point)
        {
            var x1 = linePoint1.X;
            var y1 = linePoint1.Y;
            var x2 = linePoint2.X;
            var y2 = linePoint2.Y;
            var x = point.X;
            var y = point.Y;

            var AB = Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
            var AP = Math.Sqrt((x - x1) * (x - x1) + (y - y1) * (y - y1));
            var PB = Math.Sqrt((x2 - x) * (x2 - x) + (y2 - y) * (y2 - y));
            if (AB == AP + PB)
                return true;
            else
                return false;
        }
        // Проверяет находится ли точка в многоугольнике, заданным координатами границ зоны
        public static bool IsPointInPolygon(LocationPoint point, List<AreaPoint> polygonReq)
        {
            var polygon = new List<Vector>();

            foreach (var polygonPoint in polygonReq)
            {
                polygon.Add(new Vector { X = polygonPoint.Longitude, Y = polygonPoint.Latitude });
            }
            Vector p = new Vector()
            {
                X = point.Longitude,
                Y = point.Latitude
            };

            double minX = polygon[0].X;
            double maxX = polygon[0].X;
            double minY = polygon[0].Y;
            double maxY = polygon[0].Y;
            for (int i = 1; i < polygon.Count; i++)
            {
                Vector q = polygon[i];
                minX = Math.Min(q.X, minX);
                maxX = Math.Max(q.X, maxX);
                minY = Math.Min(q.Y, minY);
                maxY = Math.Max(q.Y, maxY);
            }

            if (p.X < minX || p.X > maxX || p.Y < minY || p.Y > maxY)
            {
                return false;
            }

            bool inside = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                if ((polygon[i].Y > p.Y) != (polygon[j].Y > p.Y) &&
                     p.X < (polygon[j].X - polygon[i].X) * (p.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) + polygon[i].X)
                {
                    inside = !inside;
                }
                if (IsPointOnLine(polygon[i], polygon[j], p)) {
                    return true;
                }
            }

            return inside;
        }
        public static int RotationDirection(double p1x, double p1y, double p2x, double p2y, double p3x, double p3y)
        {
            if (((p3y - p1y) * (p2x - p1x)) > ((p2y - p1y) * (p3x - p1x)))
                return 1;
            else if (((p3y - p1y) * (p2x - p1x)) == ((p2y - p1y) * (p3x - p1x)))
                return 0;

            return -1;
        }
        public static bool ContainsSegment(double x1, double y1, double x2, double y2, double sx, double sy)
        {
            if (x1 < x2 && x1 < sx && sx < x2) return true;
            else if (x2 < x1 && x2 < sx && sx < x1) return true;
            else if (y1 < y2 && y1 < sy && sy < y2) return true;
            else if (y2 < y1 && y2 < sy && sy < y1) return true;
            else if (x1 == sx && y1 == sy || x2 == sx && y2 == sy) return true;
            return false;
        }
        // Является ли многоульник само непересекающимся
        public static bool IsSelfDiscountingPolygon(List<LocationPointRequest> points)
        {
            for (int i = 0; i < points.Count - 2; i++)
            {
                int length = points.Count;
                for (int j = i + 2; j < length; j++)
                {
                    int k = j + 1;

                    if (j == length - 1 && i == 0)
                    {
                        continue;
                    }
                    else if (j == length - 1 && i != 0)
                    {
                        k = 0;
                    }

                    if (HasIntersection(points[i], points[i + 1], points[j], points[k]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        // Пересекаются ли два отрезка, заданные точками
        public static bool HasIntersection(LocationPointRequest p1, LocationPointRequest p2, LocationPointRequest p3, LocationPointRequest p4)
        {
            double x1 = p1.Longitude;
            double y1 = p1.Latitude;
            double x2 = p2.Longitude;
            double y2 = p2.Latitude;
            double x3 = p3.Longitude;
            double y3 = p3.Latitude;
            double x4 = p4.Longitude;
            double y4 = p4.Latitude;

            var f1 = RotationDirection(x1, y1, x2, y2, x4, y4);
            var f2 = RotationDirection(x1, y1, x2, y2, x3, y3);
            var f3 = RotationDirection(x1, y1, x3, y3, x4, y4);
            var f4 = RotationDirection(x2, y2, x3, y3, x4, y4);

            // If the faces rotate opposite directions, they intersect.
            var intersect = f1 != f2 && f3 != f4;

            // If the segments are on the same line, we have to check for overlap.
            if (f1 == 0 && f2 == 0 && f3 == 0 && f4 == 0)
            {
                intersect = ContainsSegment(x1, y1, x2, y2, x3, y3) || ContainsSegment(x1, y1, x2, y2, x4, y4) ||
                ContainsSegment(x3, y3, x4, y4, x1, y1) || ContainsSegment(x3, y3, x4, y4, x2, y2);
            }

            return intersect;
        }
        // Пересекаются ли два многоугольника
        public static bool PolygonCollision(Polygon polygonA, Polygon polygonB)
        {
            var result = true;

            int edgeCountA = polygonA.Edges.Count;
            int edgeCountB = polygonB.Edges.Count;
            Vector edge;

            // Loop through all the edges of both polygons
            for (int edgeIndex = 0; edgeIndex < edgeCountA + edgeCountB; edgeIndex++)
            {
                if (edgeIndex < edgeCountA)
                {
                    edge = polygonA.Edges[edgeIndex];
                }
                else
                {
                    edge = polygonB.Edges[edgeIndex - edgeCountA];
                }

                // ===== 1. Find if the polygons are currently intersecting =====

                // Find the axis perpendicular to the current edge
                Vector axis = new Vector(-edge.Y, edge.X);
                axis.Normalize();

                // Find the projection of the polygon on the current axis
                double minA = 0; double minB = 0; double maxA = 0; double maxB = 0;
                ProjectPolygon(axis, polygonA, ref minA, ref maxA);
                ProjectPolygon(axis, polygonB, ref minB, ref maxB);

                // Check if the polygon projections are currentlty intersecting
                if (maxA < minB || maxB < minA || maxA == minB || maxB == minA)
                {
                    result = false;
                    break;
                }
            }

            return result;
        }
        // Calculate the projection of a polygon on an axis and returns it as a [min, max] interval
        public static void ProjectPolygon(Vector axis, Polygon polygon, ref double min, ref double max)
        {
            // To project a point on an axis use the dot product
            if (polygon.Points.Count == 0)
                return;
            double d = axis.DotProduct(polygon.Points[0]);
            min = d;
            max = d;
            for (int i = 0; i < polygon.Points.Count; i++)
            {
                d = polygon.Points[i].DotProduct(axis);
                if (d < min)
                {
                    min = d;
                }
                else
                {
                    if (d > max)
                    {
                        max = d;
                    }
                }
            }
        }
    }
    public struct Vector
    {

        public double X;
        public double Y;

        public Vector(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        public double Magnitude
        {
            get { return (double)Math.Sqrt(X * X + Y * Y); }
        }

        public void Normalize()
        {
            double magnitude = Magnitude;
            X = X / magnitude;
            Y = Y / magnitude;
        }

        public double DotProduct(Vector vector)
        {
            return this.X * vector.X + this.Y * vector.Y;
        }

        public static Vector operator -(Vector a, Vector b)
        {
            return new Vector(a.X - b.X, a.Y - b.Y);
        }


    }
    public class Polygon
    {

        private List<Vector> points = new List<Vector>();
        private List<Vector> edges = new List<Vector>();

        public Polygon(List<LocationPointRequest> points)
        {
            foreach (var p in points)
            {
                Points.Add(
                    new Vector
                    {
                        X = p.Longitude,
                        Y = p.Latitude
                    }
                );
            }

            BuildEdges();
        }
        public Polygon(List<AreaPoint> points)
        {
            foreach (var p in points)
            {
                Points.Add(
                    new Vector
                    {
                        X = p.Longitude,
                        Y = p.Latitude
                    }
                );
            }

            BuildEdges();
        }

        public void BuildEdges()
        {
            Vector p1;
            Vector p2;
            edges.Clear();
            for (int i = 0; i < points.Count; i++)
            {
                p1 = points[i];
                if (i + 1 >= points.Count)
                {
                    p2 = points[0];
                }
                else
                {
                    p2 = points[i + 1];
                }
                edges.Add(p2 - p1);
            }
        }

        public List<Vector> Edges
        {
            get { return edges; }
            set { edges = value; }
        }

        public List<Vector> Points
        {
            get { return points; }
            set { points = value; }
        }

    }

}
