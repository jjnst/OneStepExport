using TMPro;

public class UploadPane : NavPanel
{
	public TMP_InputField upload_name;

	public TMP_InputField upload_description;

	public override void Open()
	{
		base.Open();
		btnCtrl.RemoveFocus();
		originButton = S.I.modCtrl.newModButton;
		upload_name.text = "";
		upload_description.text = "";
	}
}
