// ORIGINAL SOURCES > http://ncase.me/sight-and-light/
// Converted to Unity > http://unitycoder.com/blog/2014/10/12/2d-visibility-shadow-for-unity-indie/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Visibility2D
{

    public class ShadowCasting2D : MonoBehaviour
    {
        public int defaultResolution = 5;//Default cone resolution
        public float visionAngle = 50f, visionDistance = .5f, securityMultiplier = 1.5f;//definido en grados

        List<Vector3> verts = new List<Vector3>();

        List<Segment2D> segments = new List<Segment2D>(); // wall line segments
        List<float> uniqueAngles = new List<float>(); // wall angles
        List<Vector3> uniquePoints = new List<Vector3>(); // wall points
        List<Intersection> intersects = new List<Intersection>(); // returned line intersects

        Mesh lightMesh;
        MeshFilter meshFilter;

        GameObject go;


        // DEBUGGING
        //		System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        //public GUIText stats;


        // INIT
        void Awake()
        {
            lightMesh = new Mesh();
            meshFilter = GetComponent<MeshFilter>();

            CollectVertices();
        } // Awake




        // Main loop
        void Update()
        {
            //Debug.DrawLine(transform.position, transform.position + transform.right * visionDistance * securityMultiplier);
            // DEBUGGING
            //stopwatch.Start();


            // Get mouse position
            //Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10));
            Vector3 mousePos = transform.position;

            // Move "player" to mouse position
            //transform.position = mousePos;


            // Get all angles
            uniqueAngles.Clear();
            for (var j = 0; j < uniquePoints.Count; j++)
            {
                //Comprobamos si el punto cae dentro del cono de visión
                if (!PointInVision(uniquePoints[j])) continue;

                float angle = Mathf.Atan2(uniquePoints[j].y - mousePos.y, uniquePoints[j].x - mousePos.x);


                AddTripleAngle(angle);
            }

            //A unique angles le tenemos que añadir los ángulos fijos del cono
            AddDefaultAngles();

            uniqueAngles.Sort();

            // Rays in all directions
            intersects.Clear();
            for (var j = 0; j < uniqueAngles.Count; j++)
            {
                //Debemos obtener el ángulo en coordenadas del mundo
                float angle = uniqueAngles[j] /*+ transform.eulerAngles.z * Mathf.Deg2Rad*/;

                // Calculate dx & dy from angle
                float dx = Mathf.Cos(angle);
                float dy = Mathf.Sin(angle);

                Ray2D ray = new Ray2D(new Vector2(mousePos.x, mousePos.y), new Vector2(mousePos.x + dx, mousePos.y + dy));
                Debug.DrawLine(transform.position, transform.position + new Vector3(dx, dy, 0) * visionDistance);
                // Find CLOSEST intersection
                Intersection closestIntersect = new Intersection();
                bool founded = false;

                for (int i = 0; i < segments.Count; i++)
                {
                    Intersection intersect = getIntersection(ray, segments[i]);

                    if (intersect.v == null) continue;

                    //Debug.Log(((intersect.v.Value - transform.position)).magnitude);

                    if (((Vector2)(intersect.v.Value - transform.position)).magnitude > visionDistance)
                    {
                        //Si la distancia es mayor que nuestro rango no vale
                        continue;
                    }

                    //if(!closestIntersect.v==null || intersect.angle<closestIntersect.angle)
                    //Aquí nop está usando angle como ángulo, sino que se ha guardado el parámetro de la recta anterior
                    if (!founded || intersect.angle < closestIntersect.angle)
                    {
                        founded = true;
                        closestIntersect = intersect;
                    }
                } // for segments

                // Intersect angle
                //if (closestIntersect == null) continue;
                if (closestIntersect.v == null)
                {
                    Vector2 uDir = new Vector2(dx, dy);//Unitary direction

                    closestIntersect.v = transform.position + (Vector3)uDir * visionDistance;
                }
                closestIntersect.angle = angle;

                // Add to list of intersects
                intersects.Add(closestIntersect);

            } // for uniqueAngles


            // Sort intersects by angle
            intersects.Sort((x, y) => { return Comparer<float?>.Default.Compare(x.angle, y.angle); });


            // Mesh generation
            List<int> tris = new List<int>();
            verts.Clear();
            tris.Clear();

            // Place first vertex at mouse position ("dummy triangulation")
            verts.Add(transform.InverseTransformPoint(transform.position));

            for (var i = 0; i < intersects.Count; i++)
            {
                if (intersects[i].v != null)
                {
                    verts.Add(transform.InverseTransformPoint((Vector3)intersects[i].v));
                    verts.Add(transform.InverseTransformPoint((Vector3)intersects[(i + 1) % intersects.Count].v));

                    //GLDebug.DrawLine((Vector3)intersects[i].v,(Vector3)intersects[(i+1) % intersects.Count].v,Color.red,0,false);
                }
            } // for intersects


            // Build triangle list
            for (var i = 0; i < verts.Count + 1; i++)
            {
                tris.Add((i + 1) % verts.Count);
                tris.Add((i) % verts.Count);
                tris.Add(0);
            }

            // Create mesh
            lightMesh.Clear();
            lightMesh.vertices = verts.ToArray();
            lightMesh.triangles = tris.ToArray();
            //lightMesh.RecalculateNormals(); // FIXME: no need if no lights..or just assign fixed value..

            meshFilter.mesh = lightMesh;

            // Debug lines from mouse to intersection
            /*
			for(var i=0;i<intersects.Count;i++)
			{
				if (intersects[i].v!=null)
				{
					GLDebug.DrawLine(new Vector3(mousePos.x,mousePos.y,0),(Vector3)intersects[i].v,Color.red,0,false);
				}
			}
			*/

            // DEBUG TIMER
            //stopwatch.Stop();
            //Debug.Log("Stopwatch: " + stopwatch.Elapsed);
            // Debug.Log("Stopwatch: " + stopwatch.ElapsedMilliseconds);
            //stats.text = ""+stopwatch.ElapsedMilliseconds+"ms";
            //stopwatch.Reset();		


        } // Update

        private void AddDefaultAngles()
        {
            //float frontAngle = Mathf.Deg2Rad * Vector2.Angle(Vector2.right, transform.right);
            float frontAngle = Mathf.Deg2Rad * transform.eulerAngles.z,
                visAngle = Mathf.Deg2Rad * visionAngle;

            for (int i=0; i<defaultResolution; i++)
            {
                float angle = frontAngle - 0.5f * visAngle + i * visAngle / (defaultResolution - 1);
                uniqueAngles.Add(angle);
            }

        }

        /*
private float Distance2(Vector3? v1, Vector3 v2)
{
   return Vector3.Distance(new Vector3(v1., v1.y))
}*/

        private void AddTripleAngle(float angle)
        {
            uniqueAngles.Add(angle - 0.001f);
            uniqueAngles.Add(angle);
            uniqueAngles.Add(angle + 0.001f);
        }

        private bool PointInVision(Vector3 point)
        {
            Vector2 distVector = point - transform.position;


            if (distVector.magnitude <= visionDistance * securityMultiplier)
            {
                //Debug.Log("Position: " + transform.position + "\nPoint: " + point + "\n distance: " + distVector.magnitude + "\nmaxDistance: " + visionDistance);

                //Si está en el rango, tiene que estar en el ángulo
                float angle = Mathf.Abs(Vector2.SignedAngle(transform.right, distVector));

                if (angle <= visionAngle / 2f)
                    return true;
            }

            return false;
        }

        void CollectVertices()
        {
            // Collect all gameobjects, with tag Wall
            GameObject[] gos = GameObject.FindGameObjectsWithTag("Wall");

            // Get all vertices from those gameobjects
            // WARNING: Should only use 2D objects, like unity Quads for now..
            foreach (GameObject go in gos)
            {
                Mesh goMesh = go.GetComponent<MeshFilter>().mesh;
                int[] tris = goMesh.triangles;

                List<int> uniqueTris = new List<int>();
                uniqueTris.Clear();

                // Collect unique tri's
                for (int i = 0; i < tris.Length; i++)
                {

                    if (!uniqueTris.Contains(tris[i]))
                    {
                        uniqueTris.Add(tris[i]);
                    }
                } // for tris


                // Sort by pseudoangle
                List<pseudoObj> all = new List<pseudoObj>();
                for (int n = 0; n < uniqueTris.Count; n++)
                {
                    float x = goMesh.vertices[uniqueTris[n]].x;
                    float y = goMesh.vertices[uniqueTris[n]].y;
                    float a = copysign(1 - x / (Mathf.Abs(x) + Mathf.Abs(y)), y);
                    pseudoObj pseudObj = new pseudoObj();
                    pseudObj.pAngle = a;
                    pseudObj.point = goMesh.vertices[uniqueTris[n]];
                    all.Add(pseudObj);
                }

                // Actual sorting
                all.Sort(delegate (pseudoObj c1, pseudoObj c2) { return c1.pAngle.CompareTo(c2.pAngle); });

                // Get unique vertices to list
                List<Vector3> uniqueVerts = new List<Vector3>();
                uniqueTris.Clear();
                for (int n = 0; n < all.Count; n++)
                {
                    uniqueVerts.Add(all[n].point);
                }

                // Get segments from unique vertices
                for (int n = 0; n < uniqueVerts.Count; n++)
                {
                    // Segment start
                    Vector3 wPos1 = go.transform.TransformPoint(uniqueVerts[n]);

                    // Segment end
                    Vector3 wPos2 = go.transform.TransformPoint(uniqueVerts[(n + 1) % uniqueVerts.Count]);

                    // TODO: duplicate of unique verts?
                    uniquePoints.Add(wPos1);

                    Segment2D seg = new Segment2D();
                    seg.a = new Vector2(wPos1.x, wPos1.y);
                    seg.b = new Vector2(wPos2.x, wPos2.y);
                    segments.Add(seg);

                    //GLDebug.DrawLine(wPos1, wPos2,Color.white,10);
                }
            } // foreach gameobject
        } // CollectVertices


        private Vector2 RotateVector(Vector2 vector, float angle)
        {
            //Convención de ángulos matemática (positivo es antihorario)

            float vectorAngle = Mathf.Deg2Rad * Vector2.Angle(Vector2.right, vector);

            return new Vector2(
                visionDistance * Mathf.Cos(vectorAngle + angle * Mathf.Deg2Rad),
                visionDistance * Mathf.Sin(vectorAngle + angle * Mathf.Deg2Rad)
                );
        }

        // Find intersection of RAY & SEGMENT
        Intersection getIntersection(Ray2D ray, Segment2D segment)
        {
            Intersection inters = new Intersection();

            // RAY in parametric: Point + Delta*T1
            float r_px = ray.a.x;
            float r_py = ray.a.y;
            float r_dx = ray.b.x - ray.a.x;
            float r_dy = ray.b.y - ray.a.y;

            // SEGMENT in parametric: Point + Delta*T2
            float s_px = segment.a.x;
            float s_py = segment.a.y;
            float s_dx = segment.b.x - segment.a.x;
            float s_dy = segment.b.y - segment.a.y;

            // Are they parallel? If so, no intersect
            var r_mag = Mathf.Sqrt(r_dx * r_dx + r_dy * r_dy);
            var s_mag = Mathf.Sqrt(s_dx * s_dx + s_dy * s_dy);

            if (r_dx / r_mag == s_dx / s_mag && r_dy / r_mag == s_dy / s_mag) // Unit vectors are the same
            {
                return inters;
            }

            // SOLVE FOR T1 & T2
            // r_px+r_dx*T1 = s_px+s_dx*T2 && r_py+r_dy*T1 = s_py+s_dy*T2
            // ==> T1 = (s_px+s_dx*T2-r_px)/r_dx = (s_py+s_dy*T2-r_py)/r_dy
            // ==> s_px*r_dy + s_dx*T2*r_dy - r_px*r_dy = s_py*r_dx + s_dy*T2*r_dx - r_py*r_dx
            // ==> T2 = (r_dx*(s_py-r_py) + r_dy*(r_px-s_px))/(s_dx*r_dy - s_dy*r_dx)
            var T2 = (r_dx * (s_py - r_py) + r_dy * (r_px - s_px)) / (s_dx * r_dy - s_dy * r_dx);
            var T1 = (s_px + s_dx * T2 - r_px) / r_dx;

            // Must be within parametic whatevers for RAY/SEGMENT
            if (T1 < 0) return inters;
            if (T2 < 0 || T2 > 1) return inters;

            inters.v = new Vector3(r_px + r_dx * T1, r_py + r_dy * T1, 0);
            inters.angle = T1;

            // Return the POINT OF INTERSECTION
            return inters;

        } // getIntersection



        // *** Helper functions ***


        // http://stackoverflow.com/questions/16542042/fastest-way-to-sort-vectors-by-angle-without-actually-computing-that-angle
        float pseudoAngle(float dx, float dy)
        {
            float ax = Mathf.Abs(dx);
            float ay = Mathf.Abs(dy);
            float p = dy / (ax + ay);
            if (dx < 0) p = 2 - p;
            //# elif dy < 0: p = 4 + p
            return p;
        }

        // http://stackoverflow.com/a/1905142
        // TODO: not likely needed to use this..
        float copysign(float a, float b)
        {
            return (a * Mathf.Sign(b));
        }

        public Vector2[] getLightPoints()
        {

            //Create an Array of vertices but avoiding the last one (which causes problems with the Edge Collider)
            if (verts.Count - 1 > 0)
            { //Check if the verts list is not 0 in order to avoid overload.

             Vector2[] verts2D = new Vector2[verts.Count - 1];

             for (int i = 0; i < verts2D.Length; i++)
             {
                 verts2D[i] = verts[i];
             }

              return verts2D;
          }
          else
          {
             return new Vector2[] { new Vector2(0, 0), new Vector2(0, 0) };
          }
        }



    } // Class
} // Namespace
