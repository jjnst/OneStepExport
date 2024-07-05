using UnityEngine;

namespace I2.Loc
{
	public class Example_LocalizedString : MonoBehaviour
	{
		public LocalizedString _MyLocalizedString;

		public string _NormalString;

		[TermsPopup("")]
		public string _StringWithTermPopup;

		public void Start()
		{
			Debug.Log(_MyLocalizedString);
			Debug.Log(LocalizationManager.GetTranslation(_NormalString));
			Debug.Log(LocalizationManager.GetTranslation(_StringWithTermPopup));
			LocalizedString localizedString = "Term2";
			string message = localizedString;
			Debug.Log(message);
			LocalizedString myLocalizedString = _MyLocalizedString;
			Debug.Log(myLocalizedString);
			LocalizedString localizedString2 = "Term3";
			Debug.Log(localizedString2);
			LocalizedString localizedString3 = "Term3";
			localizedString3.mRTL_IgnoreArabicFix = true;
			Debug.Log(localizedString3);
			LocalizedString localizedString4 = "Term3";
			localizedString4.mRTL_ConvertNumbers = true;
			localizedString4.mRTL_MaxLineLength = 20;
			Debug.Log(localizedString4);
			LocalizedString localizedString5 = localizedString4;
			Debug.Log(localizedString5);
		}
	}
}
