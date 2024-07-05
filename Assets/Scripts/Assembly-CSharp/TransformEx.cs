using UnityEngine;

public static class TransformEx
{
	public static Transform DestroyChildren(this Transform transform)
	{
		foreach (Transform item in transform)
		{
			Object.Destroy(item.gameObject);
		}
		return transform;
	}

	public static Transform DespawnChildren(this Transform transform)
	{
		foreach (Transform item in transform)
		{
			SimplePool.Despawn(item.gameObject);
		}
		return transform;
	}

	public static void SetLeft(this RectTransform rt, float left)
	{
		rt.offsetMin = new Vector2(left, rt.offsetMin.y);
	}

	public static void SetRight(this RectTransform rt, float right)
	{
		rt.offsetMax = new Vector2(0f - right, rt.offsetMax.y);
	}

	public static void SetTop(this RectTransform rt, float top)
	{
		rt.offsetMax = new Vector2(rt.offsetMax.x, 0f - top);
	}

	public static void SetBottom(this RectTransform rt, float bottom)
	{
		rt.offsetMin = new Vector2(rt.offsetMin.x, bottom);
	}

	public static void SetAll(this RectTransform rt, float all)
	{
		rt.offsetMin = new Vector2(all, all);
		rt.offsetMax = new Vector2(all, all);
	}
}
