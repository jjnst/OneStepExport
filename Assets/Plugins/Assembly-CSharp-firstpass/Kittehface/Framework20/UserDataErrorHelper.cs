using UnityEngine;

namespace Kittehface.Framework20
{
	public static class UserDataErrorHelper
	{
		public enum NoSaveSpaceMessage
		{
			CanContinue = 0,
			CannotContinue = 1
		}

		public enum CorruptDataMessage
		{
			Corrupt = 0,
			CorruptAndCreate = 1,
			CorruptAndDelete = 2
		}

		public enum UserDataOperationType
		{
			Save = 0,
			Load = 1
		}

		public enum UserResponseType
		{
			Ok = 0,
			YesNo = 1,
			OKCancel = 2
		}

		public enum UserResponse
		{
			None = 0,
			Cancel = 1,
			Yes = 2,
			No = 3
		}

		public delegate void NoSaveSpaceDialogComplete(Profiles.Profile profile, UserData.File file, bool success);

		public delegate void CorruptDataDialogComplete(Profiles.Profile profile, UserData.File file, UserResponse userResponse, bool success);

		public delegate void UserMessageDialogComplete(Profiles.Profile profile, UserResponse userResponse, bool success);

		public static event NoSaveSpaceDialogComplete OnNoSaveSpaceDialogComplete;

		public static event CorruptDataDialogComplete OnCorruptDataDialogComplete;

		public static event UserMessageDialogComplete OnUserMessageDialogComplete;

		public static bool DialogInProgress()
		{
			return false;
		}

		public static bool OpenNoSaveSpaceDialog(Profiles.Profile profile, UserData.File file, NoSaveSpaceMessage noSaveSpaceMessage)
		{
			Debug.Log("UserDataErrorHelper.OpenNoSaveSpaceDialog");
			return false;
		}

		public static bool OpenCorruptDataDialog(Profiles.Profile profile, UserData.File file, CorruptDataMessage corruptDataMessage)
		{
			Debug.Log("UserDataErrorHelper.OpenCorruptDataDialog");
			return false;
		}

		public static bool OpenUserMessageDialog(Profiles.Profile profile, string message, UserDataOperationType userDataOperationType, UserResponseType userResponseType, UserData.File file = null)
		{
			return false;
		}
	}
}
