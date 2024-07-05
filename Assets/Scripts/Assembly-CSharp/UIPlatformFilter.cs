using System;
using UnityEngine;

public class UIPlatformFilter : MonoBehaviour
{
	public class PlatformEnumFlagsAttribute : PropertyAttribute
	{
	}

	[Flags]
	public enum Platform
	{
		StandaloneWindows = 1,
		StandaloneOSX = 2,
		StandaloneLinux = 4,
		Android = 8,
		iOS = 0x10,
		tvOS = 0x20,
		XboxOne = 0x40,
		PS4 = 0x80,
		Switch = 0x100,
		StandaloneOSXAppleArcade = 0x200
	}

	[PlatformEnumFlags]
	public Platform platforms;

	private void Awake()
	{
		Refresh();
	}

	private void OnEnable()
	{
		Refresh();
	}

	private void Refresh()
	{
		if (Application.isPlaying)
		{
			bool flag = (platforms & Platform.StandaloneWindows) > (Platform)0;
			if (!flag)
			{
				RemoveFromNeighbors();
			}
			base.gameObject.SetActive(flag);
		}
	}

	private void RemoveFromNeighbors()
	{
		UIButton component = base.gameObject.GetComponent<NavButton>();
		if (component == null)
		{
			return;
		}
		UIButton up = component.up;
		UIButton down = component.down;
		UIButton left = component.left;
		UIButton right = component.right;
		NavPanel componentInParent = base.gameObject.GetComponentInParent<NavPanel>();
		if (componentInParent != null)
		{
			UIButton defaultButton = componentInParent.defaultButton;
			if (component == defaultButton)
			{
				if (down != null)
				{
					componentInParent.defaultButton = down;
				}
				else if (up != null)
				{
					componentInParent.defaultButton = up;
				}
				else if (right != null)
				{
					componentInParent.defaultButton = right;
				}
				else if (left != null)
				{
					componentInParent.defaultButton = left;
				}
				else
				{
					componentInParent.defaultButton = null;
				}
			}
		}
		if (component.up != null)
		{
			component.up.down = down;
			component.up = null;
		}
		if (component.down != null)
		{
			component.down.up = up;
			component.down = null;
		}
		if (component.left != null)
		{
			component.left.right = right;
			component.left = null;
		}
		if (component.right != null)
		{
			component.right.left = left;
			component.right = null;
		}
	}
}
