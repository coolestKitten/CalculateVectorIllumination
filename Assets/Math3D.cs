using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Math3D : MonoBehaviour
{
    public static float dot(Vector3 a, Vector3 b)
    {
        return a.x * b.x + a.y * b.y + a.z * b.z;
    }

    public static Vector3 cross(Vector3 a, Vector3 b)
    {
        Vector3 c = new Vector3();
        c.x = a.y * b.z - a.z * b.y;
        c.y = a.z * b.x - a.x * b.z;
        c.z = a.x * b.y - a.y * b.x;

        return c;
    }

    public static float length(Vector3 a)
    {
        return Mathf.Sqrt(dot(a, a));
    }

    public static Vector3 normalize(Vector3 a)
    {
        float l = length(a);
        return new Vector3(a.x / l, a.y / l, a.z / l);
    }
}
