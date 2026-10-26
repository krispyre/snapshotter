using UnityEngine;

public static class RendererExtensions
{
    // get the min max of the boundingbox projected onto the cam, idk why need so many overloads but whatever....
    public static Rect GetScreenRect(this Renderer renderer, Camera camera)
    {
        if (renderer == null || camera == null) return new Rect();
        return GetScreenRect(renderer.bounds, camera);
    }

    public static Rect GetScreenRect(this Collider collider, Camera camera)
    {
        if (collider == null || camera == null) return new Rect();
        return GetScreenRect(collider.bounds, camera);
    }

    public static Rect GetScreenRect(Bounds bounds, Camera camera)
    {
        if (camera == null) return new Rect();

        Vector3 cen = bounds.center;
        Vector3 ext = bounds.extents;

        Vector3[] worldCorners =
        {
            new Vector3(cen.x - ext.x, cen.y - ext.y, cen.z - ext.z),
            new Vector3(cen.x + ext.x, cen.y - ext.y, cen.z - ext.z),
            new Vector3(cen.x - ext.x, cen.y + ext.y, cen.z - ext.z),
            new Vector3(cen.x + ext.x, cen.y + ext.y, cen.z - ext.z),
            new Vector3(cen.x - ext.x, cen.y - ext.y, cen.z + ext.z),
            new Vector3(cen.x + ext.x, cen.y - ext.y, cen.z + ext.z),
            new Vector3(cen.x - ext.x, cen.y + ext.y, cen.z + ext.z),
            new Vector3(cen.x + ext.x, cen.y + ext.y, cen.z + ext.z)
        };

        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;

        foreach (Vector3 corner in worldCorners)
        {
            Vector3 screenPoint = camera.WorldToScreenPoint(corner);

            if (screenPoint.x < minX) minX = screenPoint.x;
            if (screenPoint.x > maxX) maxX = screenPoint.x;
            if (screenPoint.y < minY) minY = screenPoint.y;
            if (screenPoint.y > maxY) maxY = screenPoint.y;
        }

        return Rect.MinMaxRect(minX, minY, maxX, maxY);
    }
}
