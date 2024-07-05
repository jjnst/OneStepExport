using UnityEngine;

namespace AssetBundles
{
	public class AssetBundleLoadAssetOperationSimulation : AssetBundleLoadAssetOperation
	{
		private Object m_SimulatedObject;

		private bool m_isDone;

		public AssetBundleLoadAssetOperationSimulation(Object simulatedObject)
		{
			m_SimulatedObject = simulatedObject;
		}

		public override T GetAsset<T>()
		{
			return m_SimulatedObject as T;
		}

		public override bool Update()
		{
			m_isDone = true;
			CompleteOperation(null);
			return false;
		}

		public override bool IsDone()
		{
			return m_isDone;
		}
	}
}
