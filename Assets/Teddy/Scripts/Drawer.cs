using UnityEngine;

using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using mattatz.Utils;
using mattatz.Triangulation2DSystem;

namespace mattatz.TeddySystem.Example
{

    public enum OperationMode
    {
        Default,
        Draw,
        Move
    };

    public class Drawer : MonoBehaviour
    {

        [SerializeField, Range(0.2f, 1.5f)] float threshold = 1.0f;
        [SerializeField] GameObject prefab;
        [SerializeField] GameObject floor;
        [SerializeField] Material lineMat;
        [SerializeField] TextAsset json;

        public OperationMode mode;

        Teddy teddy;
        List<Vector3> points;
        List<Puppet> puppets = new List<Puppet>();

        Camera cam;
        float screenZ = 0f;

        Puppet selected;
        Vector3 origin;
        Vector3 startPoint;

        void Start()
        {
            cam = Camera.main;
            screenZ = Mathf.Abs(cam.transform.position.z - transform.position.z);

            points = new List<Vector3>();
            if (json != null)
            {
                points = JsonUtility.FromJson<JsonSerialization<Vector3>>(json.text).ToList();
                Build();
            }
        }

        void Update()
        {
            // var bottom = cam.ViewportToWorldPoint(new Vector3(0.5f, 0f, screenZ));
            // if (floor != null)
            //     floor.transform.position = bottom;

            var screen = Input.mousePosition;
            screen.z = screenZ;

            switch (mode)
            {

                case OperationMode.Default:

                    if (Input.GetMouseButtonDown(0))
                    {
                        Clear();

                        var ray = cam.ScreenPointToRay(screen);
                        RaycastHit hit;
                        if (Physics.Raycast(ray.origin, ray.direction, out hit, float.MaxValue))
                        {
                            startPoint = cam.ScreenToWorldPoint(screen); ;

                            selected = hit.collider.GetComponent<Puppet>();
                            if (selected != null)
                            {
                                selected.Select();
                                startPoint = hit.point;
                                origin = selected.transform.position;

                                mode = OperationMode.Move;
                            }
                        }
                        else
                        {
                            mode = OperationMode.Draw;
                        }
                    }

                    break;

                case OperationMode.Draw:
                    if (Input.GetMouseButtonUp(0))
                    {
                        Build();
                        mode = OperationMode.Default;
                    }
                    else
                    {
                        Vector3 p = cam.ScreenToWorldPoint(screen);
                        Vector3 p2D = new Vector3(p.x, p.y, p.z);
                        if (points.Count <= 0 || Vector3.Distance(p2D, points.Last()) > threshold)
                        {
                            points.Add(p2D);

                        }
                    }
                    break;

                case OperationMode.Move:

                    if (Input.GetMouseButtonUp(0))
                    {
                        selected.Unselect();
                        selected = null;

                        mode = OperationMode.Default;
                    }
                    else
                    {
                        var currentPoint = cam.ScreenToWorldPoint(screen);
                        var offset = currentPoint - startPoint;
                        selected.transform.position = origin + offset;
                    }

                    break;

            }

        }

        void Build()
        {
            if (points.Count < 3) return;

            List<Vector2> points2d = new List<Vector2>();
            foreach (Vector3 v in points)
                points2d.Add(new Vector2(v.x, v.y));

            points2d = Utils2D.Constrain(points2d, threshold);
            if (points2d.Count < 3) return;

            teddy = new Teddy(points2d);
            var mesh = teddy.Build(MeshSmoothingMethod.HC, 2, 0.2f, 0.75f);
            var go = Instantiate(prefab);
            //go.transform.parent = transform;

            // Vector3 center = new Vector3(0, 0, 0);
            // float count = 0;
            // foreach (Vector3 p in points)
            // {
            //     center += p;
            //     count++;
            // }
            // Vector3 theCenter = center / count;
            // go.transform.position = theCenter;

            var puppet = go.GetComponent<Puppet>();
            if (puppet != null)
            {
                puppet.SetMesh(mesh);
                puppets.Add(puppet);
            }
            else
            {
                MeshFilter mf = go.GetComponent<MeshFilter>();
                mf.sharedMesh = mesh;
                // Vector3 center = mf.mesh.GetCenterPoint();
                // GameObject temp = new GameObject("temp");
                // temp.transform.rotation = go.transform.rotation;
                // temp.transform.position = center;
                // go.transform.parent = temp.transform;
                // temp.transform.LookAt(cam.transform);


            }
            go.transform.rotation = cam.transform.rotation;

        }

        void Clear()
        {
            points.Clear();
        }

        public void Save()
        {
            List<Vector2> points2d = new List<Vector2>();
            foreach (Vector3 v in points)
                points2d.Add(new Vector2(v.x, v.y));
            LocalStorage.SaveList<Vector2>(points2d, "points.json");
        }

        public void Reset()
        {
            puppets.ForEach(puppet =>
            {
                puppet.Ignore();
                Destroy(puppet.gameObject, 10f);
            });
            puppets.Clear();
        }

        void OnDrawGizmos()
        {
            if (points != null)
            {
                Gizmos.color = Color.white;
                points.ForEach(p =>
                {
                    Gizmos.DrawSphere(p, 0.02f);
                });
            }
        }

        void OnRenderObject()
        {

            if (points != null)
            {
                GL.PushMatrix();
                GL.MultMatrix(transform.localToWorldMatrix);
                lineMat.SetColor("_Color", Color.white);
                lineMat.SetPass(0);
                GL.Begin(GL.LINES);
                for (int i = 0, n = points.Count - 1; i < n; i++)
                {
                    GL.Vertex(points[i]); GL.Vertex(points[i + 1]);
                }
                GL.End();
                GL.PopMatrix();
            }

        }

    }

}

