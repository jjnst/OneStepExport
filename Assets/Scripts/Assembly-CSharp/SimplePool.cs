using System.Collections.Generic;
using MEC;
using UnityEngine;

public class SimplePool : MonoBehaviour
{
	public class Pool
	{
		private Stack<GameObject> inactive;

		private GameObject prefab;

		public Pool(GameObject prefab, int initialQty)
		{
			this.prefab = prefab;
			inactive = new Stack<GameObject>(initialQty);
		}

		public GameObject Spawn(Vector3 pos, Quaternion rot, Transform parent = null, bool keepWorldPos = false)
		{
			GameObject gameObject;
			if (inactive.Count == 0)
			{
				gameObject = Object.Instantiate(prefab, pos, rot);
				gameObject.AddComponent<PoolMember>().myPool = this;
			}
			else
			{
				gameObject = inactive.Pop();
				if (gameObject == null)
				{
					return Spawn(pos, rot);
				}
			}
			if (parent != null)
			{
				gameObject.transform.SetParent(parent, keepWorldPos);
			}
			else
			{
				gameObject.transform.SetParent(S.I.pool);
			}
			gameObject.transform.position = pos;
			gameObject.transform.rotation = rot;
			gameObject.SetActive(true);
			return gameObject;
		}

		public GameObject SpawnNew(Vector3 pos, Quaternion rot, Transform parent = null, bool keepWorldPos = false)
		{
			GameObject gameObject = Object.Instantiate(prefab, pos, rot);
			gameObject.AddComponent<PoolMember>().myPool = this;
			if (parent != null)
			{
				gameObject.transform.SetParent(parent, keepWorldPos);
			}
			else
			{
				gameObject.transform.SetParent(S.I.pool);
			}
			gameObject.transform.position = pos;
			gameObject.transform.rotation = rot;
			gameObject.SetActive(true);
			return gameObject;
		}

		public void Despawn(GameObject obj)
		{
			if (obj.activeSelf)
			{
				Timing.RunCoroutine(_DespawnNextFrame(obj));
				obj.SetActive(false);
			}
		}

		private IEnumerator<float> _DespawnNextFrame(GameObject obj)
		{
			if (!obj)
			{
				Debug.LogError("Obj does not exist to despawn!");
				yield break;
			}
			yield return float.NegativeInfinity;
			PushToInactive(obj);
		}

		public void PushToInactive(GameObject obj)
		{
			inactive.Push(obj);
		}
	}

	public class PoolMember : MonoBehaviour
	{
		public Pool myPool;

		private float despawnTime = 0f;

		private void OnDisable()
		{
			despawnTime = 0f;
		}

		public void Despawn(float lifeTime = 0f)
		{
			if (lifeTime == 0f)
			{
				myPool.Despawn(base.gameObject);
			}
			else
			{
				despawnTime = Time.time + lifeTime;
			}
		}

		private void Update()
		{
			if (despawnTime != 0f && despawnTime < Time.time)
			{
				myPool.Despawn(base.gameObject);
			}
		}
	}

	private const int DEFAULT_POOL_SIZE = 3;

	private static Dictionary<GameObject, Pool> pools;

	private static void Init(GameObject prefab = null, int qty = 3)
	{
		if (pools == null)
		{
			pools = new Dictionary<GameObject, Pool>();
		}
		if (prefab != null && !pools.ContainsKey(prefab))
		{
			pools[prefab] = new Pool(prefab, qty);
		}
	}

	public static void Preload(GameObject prefab, int qty = 1, Transform parent = null)
	{
		Init(prefab, qty);
		GameObject[] array = new GameObject[qty];
		for (int i = 0; i < qty; i++)
		{
			array[i] = SpawnNew(prefab, Vector3.zero, Quaternion.identity, parent);
		}
		for (int j = 0; j < qty; j++)
		{
			Despawn(array[j]);
		}
	}

	public static GameObject Spawn(GameObject prefab, Vector3 pos = default(Vector3), Quaternion rot = default(Quaternion), Transform parent = null, bool keepWorldPos = false)
	{
		Init(prefab);
		return pools[prefab].Spawn(pos, rot, parent, keepWorldPos);
	}

	public static GameObject SpawnNew(GameObject prefab, Vector3 pos = default(Vector3), Quaternion rot = default(Quaternion), Transform parent = null, bool keepWorldPos = false)
	{
		Init(prefab);
		return pools[prefab].SpawnNew(pos, rot, parent, keepWorldPos);
	}

	public static void Despawn(GameObject obj, float delay = 0f)
	{
		if (obj.GetComponent<PoolMember>() == null)
		{
			Debug.Log("Object '" + obj.name + "' wasn't spawned from a pool. Destroying it instead.");
			Object.Destroy(obj);
		}
		else
		{
			obj.GetComponent<PoolMember>().Despawn(delay);
		}
	}

	public static void PushToInactive(GameObject obj)
	{
		if (obj.GetComponent<PoolMember>() == null)
		{
			Debug.Log("Object '" + obj.name + "' wasn't spawned from a pool. Destroying it instead.");
			Object.Destroy(obj);
		}
		else
		{
			obj.GetComponent<PoolMember>().myPool.PushToInactive(obj);
		}
	}

	public static void Despawn(Projectile obj, float delay = 0f)
	{
		if (obj.GetComponent<PoolMember>() == null)
		{
			Debug.Log("Object '" + obj.gameObject.name + "' wasn't spawned from a pool. Destroying it instead.");
			Object.Destroy(obj.gameObject);
		}
		else
		{
			obj.GetComponent<PoolMember>().Despawn(delay);
		}
	}
}
