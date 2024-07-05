using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionGenerator : MonoBehaviour
{
	public GameObject explosionPrefab;

	public List<RuntimeAnimatorController> explosionAnims;

	public List<AudioClip> explosionSounds;

	public int spawnRadius = 10;

	public int spawnHeight = 20;

	private int originalRadius = 0;

	public bool testExplosion = false;

	private BC ctrl;

	private void Awake()
	{
		ctrl = S.I.batCtrl;
		originalRadius = spawnRadius;
	}

	public void ResetRadius()
	{
		spawnRadius = originalRadius;
	}

	private void Update()
	{
		if (testExplosion)
		{
			CreateExplosionString(1, Vector3.right * 10f, base.transform.rotation);
			testExplosion = false;
		}
	}

	public void CreateExplosionString(int size, Vector3 position, Quaternion rotation, Being being = null)
	{
		StartCoroutine(ExplosionString(size, position, rotation, being));
	}

	private IEnumerator ExplosionString(int size, Vector3 position, Quaternion rotation, Being being = null)
	{
		switch (size)
		{
		case 0:
		{
			for (int j = 0; j < 1; j++)
			{
				SpawnExplosion(0, position, rotation, being);
				yield return new WaitForSeconds(0.15f);
			}
			break;
		}
		case 1:
		{
			for (int k = 0; k < 3; k++)
			{
				SpawnExplosion(0, position, rotation, being);
				yield return new WaitForSeconds(0.15f);
			}
			break;
		}
		case 2:
		{
			for (int l = 0; l < 10; l++)
			{
				SpawnExplosion(0, position, rotation, being);
				yield return new WaitForSeconds(0.15f);
			}
			break;
		}
		case 3:
		{
			for (int m = 0; m < 10; m++)
			{
				SpawnExplosion(0, position, rotation, being);
				yield return new WaitForSeconds(0.2f);
			}
			break;
		}
		case 9:
		{
			for (int i = 0; i < 10; i++)
			{
				SpawnExplosion(1, position, rotation, being);
				yield return new WaitForSeconds(0.2f);
			}
			break;
		}
		}
	}

	private void SpawnExplosion(int size, Vector3 position, Quaternion rotation, Being being)
	{
		Projectile component = SimplePool.Spawn(explosionPrefab, position, rotation).GetComponent<Projectile>();
		if (being == null)
		{
			component.battleGrid = ctrl.ti.refBattleGrid;
		}
		else
		{
			component.battleGrid = being.battleGrid;
		}
		component.tag = "Effect";
		component.name = "Explosion " + size;
		component.anim.runtimeAnimatorController = explosionAnims[size];
		component.sprAnim.enabled = false;
		component.transform.position += Random.insideUnitSphere * spawnRadius + Vector3.up * spawnHeight;
		component.transform.SetParent(ctrl.transform);
		component.audioSource.outputAudioMixerGroup = S.sfxGroup;
		component.ctrl = ctrl;
		component.PlayOnce(explosionSounds[size]);
		component.Despawn(explosionAnims[size].animationClips[0].length);
	}
}
