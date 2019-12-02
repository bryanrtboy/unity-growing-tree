using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomInsideMesh : MonoBehaviour
{
    /// <summary>
    /// Assume Convexity? If not, place the Center as directed.
    /// </summary>
    public bool isConvex = false;
    public bool isOnSurface = false;


    /// <summary>
    /// Rate of spawning - press "T" to instantly spawn more spheres.
    /// </summary>
    public int pointsToAdd = 50;

    MeshFilter mF;
    List<Point> points;
    List<Vector3> leaves;

    void Start()
    {
        Generator g = FindObjectOfType<Generator>();
        TreeDrawer t = FindObjectOfType<TreeDrawer>();

        mF = GetComponent<MeshFilter>();
        points = new List<Point>();
        Populate(pointsToAdd);

        if (g != null)
        {
            leaves = new List<Vector3>();
            foreach (Point p in points)
                leaves.Add(p.pos);

            g._attractors = leaves;
            g._nbAttractors = leaves.Count;
            g.enabled = true;

            if (t)
                t.enabled = true;

            Destroy(this.gameObject);

        }

    }

    void Populate(int numRays, float duration = 0)
    {
        if (duration <= 0)
            duration = Time.deltaTime;
        if (numRays <= 0)
            numRays = 1;

        for (int i = 0; i < numRays; i++)
        {
            Vector3 point;
            if (isOnSurface)
                point = mF.mesh.GetRandomPointOnSurface() + this.transform.position;
            else
                point = isConvex ? mF.mesh.GetRandomPointInsideConvex() : mF.mesh.GetRandomPointInsideNonConvex(mF.mesh.GetCenterPoint());
            // Vector3 point = isConvex ? mF.mesh.GetRandomPointInsideConvex() : mF.mesh.GetRandomPointInsideNonConvex(center.localPosition);
            points.Add(new Point(point));
        }
    }

    void OnDrawGizmos()
    {
        if (points == null || points.Count == 0)
            return;

        foreach (Point p in points)
        {
            Gizmos.color = Color.red; // The_Helper.InterpolateColor(Color.red, Color.green, p.pos.magnitude); 

            Gizmos.DrawSphere(transform.TransformPoint(p.pos), transform.lossyScale.magnitude / 100);
        }
    }

    IEnumerator DelayedDelete(Point p)
    {
        yield return new WaitForSeconds(10);
        points.Remove(p);
    }

    struct Point
    {
        public Point(Vector3 pos)
        {
            this.pos = pos;
        }
        public Vector3 pos;
    }
}
