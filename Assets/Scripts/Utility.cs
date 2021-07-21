using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility
{
    public static Vector2 SnapTo(Vector3 v3, float snapAngle) {
        float angle = Vector3.Angle (v3, Vector3.up);
        if (angle < snapAngle / 2.0f)   
            return Vector3.up * v3.magnitude; 
        if (angle > 180.0f - snapAngle / 2.0f)
            return Vector3.down * v3.magnitude;
        float t = Mathf.Round(angle / snapAngle);
        float deltaAngle = (t * snapAngle) - angle;
        
        Vector3 axis = Vector3.Cross(Vector3.up, v3);
        Quaternion q = Quaternion.AngleAxis (deltaAngle, axis);
        return q * v3;
    }
}
