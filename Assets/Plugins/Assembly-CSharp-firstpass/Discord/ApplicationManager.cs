using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Discord
{
	public class ApplicationManager
	{
		internal struct FFIEvents
		{
		}

		internal struct FFIMethods
		{
			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void ValidateOrExitCallback(IntPtr ptr, Result result);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void ValidateOrExitMethod(IntPtr methodsPtr, IntPtr callbackData, ValidateOrExitCallback callback);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void GetCurrentLocaleMethod(IntPtr methodsPtr, StringBuilder locale);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void GetCurrentBranchMethod(IntPtr methodsPtr, StringBuilder branch);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void GetOAuth2TokenCallback(IntPtr ptr, Result result, ref OAuth2Token oauth2Token);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void GetOAuth2TokenMethod(IntPtr methodsPtr, IntPtr callbackData, GetOAuth2TokenCallback callback);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void GetTicketCallback(IntPtr ptr, Result result, [MarshalAs(UnmanagedType.LPStr)] ref string data);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void GetTicketMethod(IntPtr methodsPtr, IntPtr callbackData, GetTicketCallback callback);

			internal ValidateOrExitMethod ValidateOrExit;

			internal GetCurrentLocaleMethod GetCurrentLocale;

			internal GetCurrentBranchMethod GetCurrentBranch;

			internal GetOAuth2TokenMethod GetOAuth2Token;

			internal GetTicketMethod GetTicket;
		}

		public delegate void ValidateOrExitHandler(Result result);

		public delegate void GetOAuth2TokenHandler(Result result, ref OAuth2Token oauth2Token);

		public delegate void GetTicketHandler(Result result, ref string data);

		private IntPtr MethodsPtr;

		private object MethodsStructure;

		private FFIMethods Methods
		{
			get
			{
				if (MethodsStructure == null)
				{
					MethodsStructure = Marshal.PtrToStructure(MethodsPtr, typeof(FFIMethods));
				}
				return (FFIMethods)MethodsStructure;
			}
		}

		internal ApplicationManager(IntPtr ptr, IntPtr eventsPtr, ref FFIEvents events)
		{
			if (eventsPtr == IntPtr.Zero)
			{
				throw new ResultException(Result.InternalError);
			}
			InitEvents(eventsPtr, ref events);
			MethodsPtr = ptr;
			if (MethodsPtr == IntPtr.Zero)
			{
				throw new ResultException(Result.InternalError);
			}
		}

		private void InitEvents(IntPtr eventsPtr, ref FFIEvents events)
		{
			Marshal.StructureToPtr(events, eventsPtr, false);
		}

		public void ValidateOrExit(ValidateOrExitHandler callback)
		{
			FFIMethods.ValidateOrExitCallback validateOrExitCallback = delegate(IntPtr ptr, Result result)
			{
				Utility.Release(ptr);
				callback(result);
			};
			Methods.ValidateOrExit(MethodsPtr, Utility.Retain(validateOrExitCallback), validateOrExitCallback);
		}

		public string GetCurrentLocale()
		{
			StringBuilder stringBuilder = new StringBuilder(128);
			Methods.GetCurrentLocale(MethodsPtr, stringBuilder);
			return stringBuilder.ToString();
		}

		public string GetCurrentBranch()
		{
			StringBuilder stringBuilder = new StringBuilder(4096);
			Methods.GetCurrentBranch(MethodsPtr, stringBuilder);
			return stringBuilder.ToString();
		}

		public void GetOAuth2Token(GetOAuth2TokenHandler callback)
		{
			FFIMethods.GetOAuth2TokenCallback getOAuth2TokenCallback = delegate(IntPtr ptr, Result result, ref OAuth2Token oauth2Token)
			{
				Utility.Release(ptr);
				callback(result, ref oauth2Token);
			};
			Methods.GetOAuth2Token(MethodsPtr, Utility.Retain(getOAuth2TokenCallback), getOAuth2TokenCallback);
		}

		public void GetTicket(GetTicketHandler callback)
		{
			FFIMethods.GetTicketCallback getTicketCallback = delegate(IntPtr ptr, Result result, ref string data)
			{
				Utility.Release(ptr);
				callback(result, ref data);
			};
			Methods.GetTicket(MethodsPtr, Utility.Retain(getTicketCallback), getTicketCallback);
		}
	}
}
