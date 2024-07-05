namespace Kittehface.Framework20
{
	public static class ResultExtensions
	{
		public static bool Contains(this UserData.Result value, UserData.Result resultQuery)
		{
			return (value & resultQuery) == resultQuery;
		}

		public static bool IsSuccess(this UserData.Result value)
		{
			return value.Contains(UserData.Result.Success);
		}

		public static bool IsFailure(this UserData.Result value)
		{
			return value.Contains(UserData.Result.Failure);
		}
	}
}
