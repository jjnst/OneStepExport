using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using Steamworks;
using UnityEngine;

namespace Kittehface.Framework20
{
	public class UserData
	{
		public enum ReconciliationOperationType
		{
			Load = 0,
			Save = 1
		}

		public enum ReconciliationOnlineState
		{
			Offline = 0,
			Online = 1
		}

		public enum ReconciliationResolutionType
		{
			None = 0,
			Local = 1,
			Remote = 2
		}

		public class ReconciliationEventArgs
		{
			public ReconciliationOperationType operationType;

			public ReconciliationOnlineState onlineState;

			public File file;

			public File localFile;

			public File remoteFile;
		}

		public delegate void RequestReconciliation(ReconciliationEventArgs eventArgs);

		private class Encryption
		{
			private byte[] saltBytes = null;

			private const int keysize = 256;

			private byte[] p1 = null;

			private byte[] p2 = null;

			public Encryption()
			{
			}

			public Encryption(byte[] saltBytes, byte[] p1, byte[] p2)
			{
				this.saltBytes = saltBytes;
				this.p1 = p1;
				this.p2 = p2;
			}

			public void LoadEncryptionDefinition(FileDefinition.EncryptionDefinition encryptionDefinition)
			{
				if (encryptionDefinition != null)
				{
					saltBytes = encryptionDefinition.saltBytes;
					p1 = encryptionDefinition.p1;
					p2 = encryptionDefinition.p2;
				}
			}

			public byte[] Encrypt(byte[] unencryptedData)
			{
				if (unencryptedData == null || saltBytes == null || p1 == null || p2 == null)
				{
					return null;
				}
				try
				{
					byte[] array = new byte[p1.Length];
					for (int i = 0; i < p1.Length; i++)
					{
						array[i] = (byte)(p1[i] ^ p2[i]);
					}
					string @string = Encoding.ASCII.GetString(array);
					string s = Convert.ToBase64String(unencryptedData);
					Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(@string, saltBytes);
					byte[] bytes = rfc2898DeriveBytes.GetBytes(32);
					byte[] bytes2 = rfc2898DeriveBytes.GetBytes(16);
					using (RijndaelManaged rijndaelManaged = new RijndaelManaged())
					{
						rijndaelManaged.Mode = CipherMode.CBC;
						using (ICryptoTransform transform = rijndaelManaged.CreateEncryptor(bytes, bytes2))
						{
							using (MemoryStream memoryStream = new MemoryStream())
							{
								using (CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write))
								{
									byte[] bytes3 = Encoding.UTF8.GetBytes(s);
									cryptoStream.Write(bytes3, 0, bytes3.Length);
									cryptoStream.FlushFinalBlock();
									return memoryStream.ToArray();
								}
							}
						}
					}
				}
				catch (Exception ex)
				{
					Log(VerbosityLevel.Error, "Error encrypting: " + ex.Message + "\n" + ex.StackTrace);
				}
				return null;
			}

			public byte[] Decrypt(byte[] encryptedData)
			{
				if (encryptedData == null || saltBytes == null || p1 == null || p2 == null)
				{
					return null;
				}
				try
				{
					byte[] array = new byte[p1.Length];
					for (int i = 0; i < p1.Length; i++)
					{
						array[i] = (byte)(p1[i] ^ p2[i]);
					}
					string @string = Encoding.ASCII.GetString(array);
					Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(@string, saltBytes);
					byte[] bytes = rfc2898DeriveBytes.GetBytes(32);
					byte[] bytes2 = rfc2898DeriveBytes.GetBytes(16);
					using (RijndaelManaged rijndaelManaged = new RijndaelManaged())
					{
						rijndaelManaged.Mode = CipherMode.CBC;
						using (ICryptoTransform transform = rijndaelManaged.CreateDecryptor(bytes, bytes2))
						{
							using (MemoryStream stream = new MemoryStream(encryptedData))
							{
								using (CryptoStream cryptoStream = new CryptoStream(stream, transform, CryptoStreamMode.Read))
								{
									byte[] array2 = new byte[encryptedData.Length];
									int count = cryptoStream.Read(array2, 0, array2.Length);
									string string2 = Encoding.UTF8.GetString(array2, 0, count);
									return Convert.FromBase64String(string2);
								}
							}
						}
					}
				}
				catch (Exception ex)
				{
					Log(VerbosityLevel.Error, "Error decrypting: " + ex.Message + "\n" + ex.StackTrace);
				}
				return null;
			}
		}

		public interface File
		{
			Profiles.Profile owner { get; }

			string filename { get; }

			bool cloudEnabled { get; }

			bool useEncryption { get; }

			bool prettyPrint { get; }

			bool useBinarySerialization { get; }

			DateTime fileDateTime { get; }

			ulong fileRequiredBlocks { get; }

			string directoryName { get; }

			event Action<File, Result> OnReadMetadataCompleted;

			event Action<File, Result> OnReadCompleted;

			event Action<File, Result> OnWriteCompleted;

			event Action<File, Result> OnUnmountCompleted;

			event Action<File, Result> OnDeleteCompleted;

			event RequestReconciliation OnRequestReconciliation;

			void Set<T>(string key, T value, WriteMode writeMode = WriteMode.System);

			T Get<T>(string key, T defaultValue = default(T));

			bool Contains(string key);

			void Remove(string key, WriteMode writeMode = WriteMode.System);

			void RemoveAll(WriteMode writeMode = WriteMode.System);

			void ReadMetadata();

			void Read();

			void Write();

			void Unmount();

			void Delete();

			void Reconcile(ReconciliationResolutionType resolutionType);
		}

		private class OperableFile : File
		{
			private Encryption encryption = new Encryption();

			private object fileOperationLock = new object();

			private FileOperation currentFileOperation = null;

			private Hashtable data;

			private object dataLock = new object();

			[Obsolete("Should not be accessed directly. Use the 'modified' property.")]
			private bool _modified = false;

			private Thread operationThread = null;

			public Profiles.Profile owner { get; set; }

			public string filename { get; set; }

			public bool cloudEnabled { get; set; }

			public bool useEncryption { get; set; }

			public bool prettyPrint { get; set; }

			public bool useBinarySerialization { get; set; }

			public DateTime fileDateTime { get; set; }

			public ulong fileRequiredBlocks { get; set; }

			public string directoryName { get; set; }

			public bool unmountInProgress { get; private set; }

			public bool modified
			{
				get
				{
					lock (dataLock)
					{
						return _modified;
					}
				}
				set
				{
					lock (dataLock)
					{
						_modified = value;
					}
				}
			}

			public event Action<File, Result> OnReadMetadataCompleted;

			public event Action<File, Result> OnReadCompleted;

			public event Action<File, Result> OnWriteCompleted;

			public event Action<File, Result> OnUnmountCompleted;

			public event Action<File, Result> OnDeleteCompleted;

			public event RequestReconciliation OnRequestReconciliation;

			public void LoadFileDefinition(FileDefinition fileDefinition)
			{
				if (fileDefinition != null)
				{
					cloudEnabled = fileDefinition.cloudEnabled;
					useEncryption = fileDefinition.useEncryption;
					prettyPrint = fileDefinition.prettyPrint;
					useBinarySerialization = fileDefinition.useBinarySerialization;
					if (useBinarySerialization && prettyPrint)
					{
						Log(VerbosityLevel.Warning, "UserData.OperableFile.LoadFileDefinition: pretty print set with binary serialization, pretty print will not work.");
					}
					encryption.LoadEncryptionDefinition(fileDefinition.encryptionDefinition);
				}
			}

			private bool DataAccessible()
			{
				lock (dataLock)
				{
					if (data == null)
					{
						Log(VerbosityLevel.Info, "UserData.File:DataAccessible() Creating new data Hashtable");
						data = new Hashtable();
					}
				}
				return true;
			}

			public void Set<T>(string key, T value, WriteMode writeMode = WriteMode.System)
			{
				if (string.IsNullOrEmpty(key))
				{
					throw new ArgumentException("Parameter 'key' cannot be null or empty.", "key");
				}
				Log(VerbosityLevel.Info, "UserData.OperableFile.Set( key = " + key + " )");
				if (DataAccessible())
				{
					lock (dataLock)
					{
						data[key] = value;
						modified = true;
					}
					if (((WriteMode.System == writeMode) ? SystemWriteMode : writeMode) == WriteMode.Immediate)
					{
						Write();
					}
					return;
				}
				throw new InvalidOperationException("Set cannot be called before file data has been loaded.");
			}

			public T Get<T>(string key, T defaultValue = default(T))
			{
				if (string.IsNullOrEmpty(key))
				{
					throw new ArgumentException("Parameter 'key' cannot be null or empty.", "key");
				}
				if (DataAccessible())
				{
					T result = defaultValue;
					lock (dataLock)
					{
						if (data.ContainsKey(key))
						{
							object obj = data[key];
							if (obj != null)
							{
								if (obj.GetType().IsSubclassOf(typeof(T)) || obj.GetType() == typeof(T))
								{
									result = (T)obj;
								}
								else
								{
									Debug.LogWarning("UserData Warning: [" + key + "] value type [" + obj.GetType().ToString() + "] does not match the requested value type [" + typeof(T).ToString() + "]");
								}
							}
						}
					}
					return result;
				}
				throw new InvalidOperationException("Get cannot be called before file data has been loaded.");
			}

			public bool Contains(string key)
			{
				if (string.IsNullOrEmpty(key))
				{
					throw new ArgumentException("Parameter 'key' cannot be null or empty.", "key");
				}
				if (DataAccessible())
				{
					lock (dataLock)
					{
						return data.ContainsKey(key);
					}
				}
				throw new InvalidOperationException("Contains cannot be called before file data has been loaded.");
			}

			public void Remove(string key, WriteMode writeMode = WriteMode.System)
			{
				if (string.IsNullOrEmpty(key))
				{
					throw new ArgumentException("Parameter 'key' cannot be null or empty.", "key");
				}
				if (DataAccessible())
				{
					lock (dataLock)
					{
						data.Remove(key);
						modified = true;
						if (((WriteMode.System == writeMode) ? SystemWriteMode : writeMode) == WriteMode.Immediate)
						{
							Write();
						}
						return;
					}
				}
				throw new InvalidOperationException("Remove cannot be called before file data has been loaded.");
			}

			public void RemoveAll(WriteMode writeMode = WriteMode.System)
			{
				if (DataAccessible())
				{
					lock (dataLock)
					{
						data.Clear();
						modified = true;
						if (((WriteMode.System == writeMode) ? SystemWriteMode : writeMode) == WriteMode.Immediate)
						{
							Write();
						}
						return;
					}
				}
				throw new InvalidOperationException("RemoveAll cannot be called before file data has been loaded.");
			}

			public void ExecuteOperation(FileOperation fileOperation)
			{
				Log(VerbosityLevel.Info, string.Concat("UserData:OperableFile:ExecuteOperation( fileOperation.operationType = ", fileOperation.action, " )"));
				lock (fileOperationLock)
				{
					if (null != currentFileOperation)
					{
						throw new InvalidOperationException("Only one operation can be performed on a File at a time.");
					}
					currentFileOperation = fileOperation;
				}
				Log(VerbosityLevel.Info, "UserData:OperableFile:ExecuteOperation(): Subscribing to Update");
				Platform.OnUpdate += Update;
				ExecuteOperation_Platform();
			}

			private void ExecuteOperation_Platform()
			{
				Log(VerbosityLevel.Info, "UserData.OperableFile.Execute_Platform()");
				lock (fileOperationLock)
				{
					if (null == currentFileOperation)
					{
						throw new InvalidOperationException("Execute_Platform cannot be called if currentOperation is null.");
					}
					switch (currentFileOperation.action)
					{
					case Operation.Action.Mount:
						operationThread = new Thread(ExecuteMount);
						operationThread.Start();
						Log(VerbosityLevel.Info, "UserData.OperableFile.Execute_Platform(): Mount operation started.");
						break;
					case Operation.Action.ReadMetadata:
						operationThread = new Thread(ExecuteReadMetadata);
						operationThread.Start();
						Log(VerbosityLevel.Info, "UserData.OperableFile.Execute_Platform(): Read metadata operation started.");
						break;
					case Operation.Action.Read:
						operationThread = new Thread(ExecuteRead);
						operationThread.Start();
						Log(VerbosityLevel.Info, "UserData.OperableFile.Execute_Platform(): Read operation started.");
						break;
					case Operation.Action.Write:
						operationThread = new Thread(ExecuteWrite);
						operationThread.Start();
						Log(VerbosityLevel.Info, "UserData.OperableFile.Execute_Platform(): Write operation started.");
						break;
					case Operation.Action.Unmount:
						operationThread = new Thread(ExecuteUnmount);
						operationThread.Start();
						Log(VerbosityLevel.Info, "UserData.OperableFile.Execute_Platform(): Unmount operation started.");
						break;
					case Operation.Action.Delete:
						operationThread = new Thread(ExecuteDelete);
						operationThread.Start();
						Log(VerbosityLevel.Info, "UserData.OperableFile.Execute_Platform(): Delete operation started.");
						break;
					}
				}
			}

			private void CompleteOperation_Platform(FileOperation completedOperation)
			{
				Log(VerbosityLevel.Info, "UserData.OperableFile.CompleteOperation_Platform()");
			}

			private void Update()
			{
				lock (fileOperationLock)
				{
					if (currentFileOperation.complete)
					{
						Log(VerbosityLevel.Info, "UserData.OperableFile.Update(): Operation complete.");
						FileOperation fileOperation = currentFileOperation;
						currentFileOperation = null;
						Log(VerbosityLevel.Info, "UserData:OperableFile:ExecuteOperation(): Unsubscribing from Update");
						Platform.OnUpdate -= Update;
						CompleteOperation_Platform(fileOperation);
						fileOperation.NotifyComplete();
					}
				}
			}

			public void ExecuteMount()
			{
				FileOperation fileOperation;
				lock (fileOperationLock)
				{
					fileOperation = currentFileOperation;
				}
				try
				{
					Log(VerbosityLevel.Info, "UserData.OperableFile.ExecuteMount()");
					try
					{
						ExecuteMount_Platform();
						lock (fileOperationLock)
						{
							if (fileOperation.complete)
							{
								return;
							}
						}
					}
					catch (Exception ex)
					{
						Log(VerbosityLevel.Error, ex.Message + "\n" + ex.StackTrace);
						lock (fileOperationLock)
						{
							fileOperation.Fail(Result.Failure);
							return;
						}
					}
					Profiles.OnLateWillDeactivate += Profile_OnLateWillDeactivate;
					lock (fileOperationLock)
					{
						fileOperation.Complete();
					}
				}
				catch (Exception ex2)
				{
					Log(VerbosityLevel.Error, ex2.Message + "\n" + ex2.StackTrace);
				}
			}

			private void ExecuteMount_Platform()
			{
				Log(VerbosityLevel.Info, "UserData.OperableFile.Mount_Platform()");
			}

			public void ReadMetadata()
			{
				Log(VerbosityLevel.Info, "UserData.OperableFile.ReadMetadata()");
				RequestReadMetadata(this);
			}

			public void ExecuteReadMetadata()
			{
				FileOperation fileOperation;
				lock (fileOperationLock)
				{
					fileOperation = currentFileOperation;
				}
				try
				{
					try
					{
						ReadMetadata_Platform();
						lock (fileOperationLock)
						{
							if (fileOperation.complete)
							{
								return;
							}
						}
					}
					catch (Exception ex)
					{
						Log(VerbosityLevel.Error, ex.Message + "\n" + ex.StackTrace);
						lock (fileOperationLock)
						{
							fileOperation.Fail(Result.Failure);
							return;
						}
					}
					lock (fileOperationLock)
					{
						fileOperation.Complete();
					}
				}
				catch (Exception ex2)
				{
					Log(VerbosityLevel.Error, ex2.Message + "\n" + ex2.StackTrace);
				}
			}

			private void ReadMetadata_Platform()
			{
				Log(VerbosityLevel.Info, "UserData.OperableFile.ReadMetadata_Platform()");
				if (!cloudEnabled)
				{
					ReadMetadata_Default("");
					return;
				}
				if (Platform.isEditor && !Platform.RunSteamInEditor)
				{
					ReadMetadata_Default("Steam");
					return;
				}
				if (!Platform.SteamInitialized)
				{
					lock (fileOperationLock)
					{
						currentFileOperation.Fail(Result.Failure);
						return;
					}
				}
				if (SteamRemoteStorage.FileExists(filename))
				{
					long fileTimestamp = SteamRemoteStorage.GetFileTimestamp(filename);
					fileDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
					fileDateTime = fileDateTime.AddSeconds(fileTimestamp);
					return;
				}
				lock (fileOperationLock)
				{
					currentFileOperation.AddResult(Result.FileNotFound);
				}
			}

			public void NotifyReadMetadataCompleted(Result result)
			{
				Log(VerbosityLevel.Info, "UserData.OperableFile.NotifyReadMetadataCompleted( result = " + result.ToString() + " )");
				if (this.OnReadMetadataCompleted != null)
				{
					this.OnReadMetadataCompleted(this, result);
				}
			}

			public void Read()
			{
				Log(VerbosityLevel.Info, "UserData.OperableFile.Read()");
				RequestRead(this);
			}

			public void ExecuteRead()
			{
				FileOperation fileOperation;
				lock (fileOperationLock)
				{
					fileOperation = currentFileOperation;
				}
				try
				{
					if (data != null)
					{
						Log(VerbosityLevel.Warning, "Reading a file with existing data will overwrite the data with the contents of system storage.");
					}
					byte[] serializedData = null;
					try
					{
						Read_Platform(ref serializedData);
						lock (fileOperationLock)
						{
							if (fileOperation.complete)
							{
								return;
							}
						}
					}
					catch (Exception ex)
					{
						Log(VerbosityLevel.Error, ex.Message + "\n" + ex.StackTrace);
						lock (fileOperationLock)
						{
							fileOperation.Fail(Result.Failure);
							return;
						}
					}
					if (serializedData != null)
					{
						try
						{
							if (useEncryption && SupportsEncryption)
							{
								byte[] array = encryption.Decrypt(serializedData);
								if (array == null)
								{
									lock (fileOperationLock)
									{
										fileOperation.Fail(Result.FailEncryption);
									}
									Log(VerbosityLevel.Info, "UserData.OperableFile.ExecuteAsyncRead(): Read data failed to decrypt, creating new hash table.");
									lock (dataLock)
									{
										data = new Hashtable();
										return;
									}
								}
								serializedData = array;
							}
							Deserialize(serializedData);
						}
						catch (Exception ex2)
						{
							Log(VerbosityLevel.Error, ex2.Message + "\n" + ex2.StackTrace);
							lock (dataLock)
							{
								data = null;
							}
						}
					}
					if (data == null)
					{
						Log(VerbosityLevel.Info, "UserData.OperableFile.ExecuteAsyncRead(): Read data was null, creating new hash table.");
						lock (dataLock)
						{
							data = new Hashtable();
						}
					}
					lock (fileOperationLock)
					{
						fileOperation.Complete();
					}
				}
				catch (Exception ex3)
				{
					Log(VerbosityLevel.Error, ex3.Message + "\n" + ex3.StackTrace);
				}
			}

			private void Read_Platform(ref byte[] serializedData)
			{
				Log(VerbosityLevel.Info, "UserData.OperableFile.Read_Platform()");
				if (serializedData != null)
				{
					throw new ArgumentException("Parameter 'serializedData' must be null. Reading into an occupied buffer is unsupported.");
				}
				if (!cloudEnabled)
				{
					Read_Default(ref serializedData, "");
					return;
				}
				if (Platform.isEditor && !Platform.RunSteamInEditor)
				{
					Read_Default(ref serializedData, "Steam");
					return;
				}
				if (!Platform.SteamInitialized)
				{
					lock (fileOperationLock)
					{
						currentFileOperation.Fail(Result.Failure);
						return;
					}
				}
				if (SteamRemoteStorage.FileExists(filename))
				{
					int fileSize = SteamRemoteStorage.GetFileSize(filename);
					serializedData = new byte[fileSize];
					int num = SteamRemoteStorage.FileRead(filename, serializedData, serializedData.Length);
					if (num == fileSize)
					{
						return;
					}
					lock (fileOperationLock)
					{
						currentFileOperation.Fail(Result.Failure);
						return;
					}
				}
				lock (fileOperationLock)
				{
					currentFileOperation.AddResult(Result.FileNotFound);
				}
			}

			public void NotifyReadCompleted(Result result)
			{
				Log(VerbosityLevel.Info, "UserData.OperableFile.NotifyReadCompleted( result = " + result.ToString() + " )");
				if (this.OnReadCompleted != null)
				{
					this.OnReadCompleted(this, result);
				}
			}

			private void Deserialize(byte[] serializedData)
			{
				Log(VerbosityLevel.Info, "UserData.OperableFile.Deserialize()");
				if (!useBinarySerialization)
				{
					DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(Hashtable), RegisteredDataTypes);
					XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
					xmlReaderSettings.IgnoreWhitespace = true;
					using (MemoryStream input = new MemoryStream(serializedData))
					{
						using (XmlReader xmlReader = XmlReader.Create(input, xmlReaderSettings))
						{
							lock (dataLock)
							{
								try
								{
									data = (Hashtable)dataContractSerializer.ReadObject(xmlReader);
								}
								catch (Exception innerException)
								{
									data = null;
									string text = "Exception Deserializing Data: " + innerException.Message;
									while (innerException.InnerException != null)
									{
										innerException = innerException.InnerException;
										text = text + "\n Inner Exception: " + innerException.Message;
									}
									Debug.LogError(text);
								}
							}
							xmlReader.Close();
							return;
						}
					}
				}
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				using (MemoryStream serializationStream = new MemoryStream(serializedData))
				{
					lock (dataLock)
					{
						try
						{
							data = (Hashtable)binaryFormatter.Deserialize(serializationStream);
						}
						catch (Exception innerException2)
						{
							data = null;
							string text2 = "Exception Deserializing Data: " + innerException2.Message;
							while (innerException2.InnerException != null)
							{
								innerException2 = innerException2.InnerException;
								text2 = text2 + "\n Inner Exception: " + innerException2.Message;
							}
							Debug.LogError(text2);
						}
					}
				}
			}

			public void Write()
			{
				Log(VerbosityLevel.Info, "UserData.OperableFile.Write()");
				RequestWrite(this);
			}

			private void ExecuteWrite()
			{
				FileOperation fileOperation;
				lock (fileOperationLock)
				{
					fileOperation = currentFileOperation;
				}
				try
				{
					Log(VerbosityLevel.Info, "UserData.OperableFile.ExecuteWrite()");
					byte[] array = null;
					try
					{
						lock (dataLock)
						{
							array = Serialize();
						}
					}
					catch (Exception ex)
					{
						Log(VerbosityLevel.Error, ex.Message + "\n" + ex.StackTrace);
					}
					if (array != null)
					{
						try
						{
							if (useEncryption && SupportsEncryption)
							{
								if (prettyPrint)
								{
									Debug.LogWarning("UserData.File.ExecuteWrite: File [" + filename + "] is encrypting data, but is set to pretty print.");
								}
								byte[] array2 = encryption.Encrypt(array);
								if (array2 == null)
								{
									lock (fileOperationLock)
									{
										fileOperation.Fail(Result.FailEncryption);
										return;
									}
								}
								array = array2;
							}
							ExecuteWrite_Platform(array);
							lock (fileOperationLock)
							{
								if (fileOperation.complete)
								{
									return;
								}
							}
						}
						catch (Exception ex2)
						{
							Log(VerbosityLevel.Error, ex2.Message + "\n" + ex2.StackTrace);
							lock (fileOperationLock)
							{
								fileOperation.Fail(Result.Failure);
								return;
							}
						}
						Log(VerbosityLevel.Info, "UserData.OperableFile.ExecuteWrite(): Clearing Modified");
						lock (dataLock)
						{
							modified = false;
						}
						Log(VerbosityLevel.Info, "UserData.OperableFile.ExecuteWrite(): Completing Operation");
						lock (fileOperationLock)
						{
							fileOperation.Complete();
							return;
						}
					}
					lock (fileOperationLock)
					{
						Log(VerbosityLevel.Error, "UserData.OperableFile.ExecuteWrite(): Error serializing data.");
						fileOperation.Fail(Result.Failure);
					}
				}
				catch (Exception ex3)
				{
					Log(VerbosityLevel.Error, ex3.Message + "\n" + ex3.StackTrace);
				}
			}

			private void ExecuteWrite_Platform(byte[] serializedData)
			{
				Log(VerbosityLevel.Info, "UserData.OperableFile.ExecuteWrite_Platform()");
				if (serializedData == null)
				{
					throw new ArgumentNullException("Argument 'serializedData' cannot be null.");
				}
				if (!cloudEnabled)
				{
					Write_Default(serializedData, "");
					return;
				}
				if (Platform.isEditor && !Platform.RunSteamInEditor)
				{
					Write_Default(serializedData, "Steam");
					return;
				}
				if (!Platform.SteamInitialized)
				{
					lock (fileOperationLock)
					{
						currentFileOperation.Fail(Result.Failure);
						return;
					}
				}
				if (SteamRemoteStorage.FileWrite(filename, serializedData, serializedData.Length))
				{
					return;
				}
				lock (fileOperationLock)
				{
					currentFileOperation.Fail(Result.Failure);
				}
			}

			public void NotifyWriteCompleted(Result result)
			{
				Log(VerbosityLevel.Info, "UserData.OperableFile.NotifyWriteCompleted( result = " + result.ToString() + " )");
				if (this.OnWriteCompleted != null)
				{
					this.OnWriteCompleted(this, result);
				}
			}

			private byte[] Serialize()
			{
				Log(VerbosityLevel.Info, "UserData.OperableFile.Serialize()");
				if (!useBinarySerialization)
				{
					DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(Hashtable), RegisteredDataTypes);
					XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
					if (prettyPrint)
					{
						xmlWriterSettings.Indent = true;
						xmlWriterSettings.OmitXmlDeclaration = false;
					}
					else
					{
						xmlWriterSettings.Indent = false;
						xmlWriterSettings.OmitXmlDeclaration = true;
					}
					using (MemoryStream memoryStream = new MemoryStream())
					{
						using (XmlWriter xmlWriter = XmlWriter.Create(memoryStream, xmlWriterSettings))
						{
							lock (dataLock)
							{
								try
								{
									dataContractSerializer.WriteObject(xmlWriter, data);
									xmlWriter.Close();
									byte[] array = memoryStream.ToArray();
									Debug.Log("Serialized Data to buffer. (Length = " + array.Length + " bytes)");
									return array;
								}
								catch (Exception innerException)
								{
									string text = "Exception Serializing Data: " + innerException.Message;
									while (innerException.InnerException != null)
									{
										innerException = innerException.InnerException;
										text = text + "\n Inner Exception: " + innerException.Message;
									}
									Debug.LogError(text);
								}
							}
							return null;
						}
					}
				}
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				using (MemoryStream memoryStream2 = new MemoryStream())
				{
					lock (dataLock)
					{
						try
						{
							binaryFormatter.Serialize(memoryStream2, data);
							memoryStream2.Close();
							byte[] array = memoryStream2.ToArray();
							Debug.Log("Serialized Data to buffer. (Length = " + array.Length + " bytes)");
							return array;
						}
						catch (Exception innerException2)
						{
							string text2 = "Exception Serializing Data: " + innerException2.Message;
							while (innerException2.InnerException != null)
							{
								innerException2 = innerException2.InnerException;
								text2 = text2 + "\n Inner Exception: " + innerException2.Message;
							}
							Debug.LogError(text2);
						}
					}
					return null;
				}
			}

			public void Unmount()
			{
				Log(VerbosityLevel.Info, "UserData.OperableFile.Unmount()");
				if (!unmountInProgress)
				{
					unmountInProgress = true;
					Profiles.OnLateWillDeactivate -= Profile_OnLateWillDeactivate;
					RequestUnmount(this);
				}
			}

			public void ExecuteUnmount()
			{
				FileOperation fileOperation;
				lock (fileOperationLock)
				{
					fileOperation = currentFileOperation;
				}
				try
				{
					Log(VerbosityLevel.Info, "UserData.OperableFile.ExecuteUnmount()");
					try
					{
						ExecuteUnmount_Platform();
						lock (fileOperationLock)
						{
							if (fileOperation.complete)
							{
								return;
							}
						}
					}
					catch (Exception ex)
					{
						Log(VerbosityLevel.Error, ex.Message + "\n" + ex.StackTrace);
						lock (fileOperationLock)
						{
							fileOperation.Fail(Result.Failure);
							return;
						}
					}
					lock (fileOperationLock)
					{
						fileOperation.Complete();
					}
				}
				catch (Exception ex2)
				{
					Log(VerbosityLevel.Error, ex2.Message + "\n" + ex2.StackTrace);
				}
			}

			private void ExecuteUnmount_Platform()
			{
				Log(VerbosityLevel.Info, "UserData.OperableFile.Unmount_Platform()");
			}

			public void NotifyUnmountCompleted(Result result)
			{
				Log(VerbosityLevel.Info, "UserData.OperableFile.NotifyUnmountCompleted( result = " + result.ToString() + " )");
				if (this.OnUnmountCompleted != null)
				{
					this.OnUnmountCompleted(this, result);
				}
			}

			public void Delete()
			{
				Log(VerbosityLevel.Info, "UserData.OperableFile.Delete()");
				RequestDelete(this);
			}

			public void ExecuteDelete()
			{
				FileOperation fileOperation;
				lock (fileOperationLock)
				{
					fileOperation = currentFileOperation;
				}
				try
				{
					Log(VerbosityLevel.Info, "UserData.OperableFile.ExecuteDelete()");
					try
					{
						ExecuteDelete_Platform();
						lock (fileOperationLock)
						{
							if (fileOperation.complete)
							{
								return;
							}
						}
						lock (dataLock)
						{
							data = null;
							Log(VerbosityLevel.Info, "UserData.OperableFile.ExecuteDelete(): Clearing Modified");
							modified = false;
						}
					}
					catch (Exception ex)
					{
						Log(VerbosityLevel.Error, ex.Message + "\n" + ex.StackTrace);
						lock (fileOperationLock)
						{
							fileOperation.Fail(Result.Failure);
							return;
						}
					}
					lock (fileOperationLock)
					{
						fileOperation.Complete();
					}
				}
				catch (Exception ex2)
				{
					Log(VerbosityLevel.Error, ex2.Message + "\n" + ex2.StackTrace);
				}
			}

			private void ExecuteDelete_Platform()
			{
				Log(VerbosityLevel.Info, "UserData.OperableFile.Delete_Platform()");
				if (!cloudEnabled)
				{
					Delete_Default("");
					return;
				}
				if (Platform.isEditor && !Platform.RunSteamInEditor)
				{
					Delete_Default("Steam");
					return;
				}
				if (!Platform.SteamInitialized)
				{
					lock (fileOperationLock)
					{
						currentFileOperation.Fail(Result.Failure);
						return;
					}
				}
				if (!SteamRemoteStorage.FileExists(filename) || SteamRemoteStorage.FileDelete(filename))
				{
					return;
				}
				lock (fileOperationLock)
				{
					currentFileOperation.Fail(Result.Failure);
				}
			}

			public void NotifyDeleteCompleted(Result result)
			{
				Log(VerbosityLevel.Info, "UserData.OperableFile.NotifyDeleteCompleted( result = " + result.ToString() + " )");
				if (this.OnDeleteCompleted != null)
				{
					this.OnDeleteCompleted(this, result);
				}
			}

			public void Reconcile(ReconciliationResolutionType resolutionType)
			{
			}

			private void Profile_OnLateWillDeactivate(Profiles.Profile profile)
			{
				if (profile == owner)
				{
					Unmount();
				}
			}

			private void ReadMetadata_Default(string platformDir)
			{
				string path = Path.Combine(Path.GetFullPath(PersistentDataPath), platformDir);
				string path2 = Path.Combine(path, filename);
				if (System.IO.File.Exists(path2))
				{
					fileDateTime = System.IO.File.GetLastWriteTimeUtc(path2);
					return;
				}
				lock (fileOperationLock)
				{
					currentFileOperation.AddResult(Result.FileNotFound);
				}
			}

			private void Read_Default(ref byte[] serializedData, string platformDir)
			{
				string text = Path.Combine(Path.GetFullPath(PersistentDataPath), platformDir);
				Log(VerbosityLevel.Info, "Application Data Path: " + text);
				string text2 = Path.Combine(text, filename);
				Log(VerbosityLevel.Info, "File Path: " + text2);
				if (System.IO.File.Exists(text2))
				{
					serializedData = System.IO.File.ReadAllBytes(text2);
					return;
				}
				lock (fileOperationLock)
				{
					currentFileOperation.AddResult(Result.FileNotFound);
				}
			}

			private void Write_Default(byte[] serializedData, string platformDir)
			{
				if (serializedData == null)
				{
					throw new ArgumentNullException("Cannot write null data.");
				}
				string text = Path.Combine(Path.GetFullPath(PersistentDataPath), platformDir);
				Log(VerbosityLevel.Info, "Application Data Path: " + text);
				string text2 = Path.Combine(text, filename);
				Log(VerbosityLevel.Info, "File Path: " + text2);
				FileStream fileStream = null;
				BinaryWriter binaryWriter = null;
				try
				{
					Directory.CreateDirectory(text);
					fileStream = System.IO.File.Open(text2, FileMode.Create, FileAccess.Write);
					binaryWriter = new BinaryWriter(fileStream);
					binaryWriter.Write(serializedData);
					binaryWriter.Flush();
					binaryWriter.Close();
				}
				catch (Exception ex)
				{
					Log(VerbosityLevel.Error, "UserData.OperableFile.Write_Editor(): " + ex.Message);
				}
				finally
				{
					if (binaryWriter != null)
					{
						binaryWriter.Close();
					}
					else if (fileStream != null)
					{
						fileStream.Close();
					}
				}
			}

			private void Delete_Default(string platformDir)
			{
				string path = Path.Combine(Path.GetFullPath(PersistentDataPath), platformDir);
				string path2 = Path.Combine(path, filename);
				if (System.IO.File.Exists(path2))
				{
					System.IO.File.Delete(path2);
					return;
				}
				lock (fileOperationLock)
				{
					currentFileOperation.AddResult(Result.FileNotFound);
				}
			}
		}

		public class FileDefinition
		{
			public class EncryptionDefinition
			{
				public byte[] saltBytes;

				public byte[] p1;

				public byte[] p2;

				public EncryptionDefinition(byte[] saltBytes, byte[] p1, byte[] p2)
				{
					this.saltBytes = saltBytes;
					this.p1 = p1;
					this.p2 = p2;
				}

				public EncryptionDefinition(EncryptionDefinition other)
				{
					saltBytes = other.saltBytes;
					p1 = other.p1;
					p2 = other.p2;
				}

				public static void GenerateSalt(out string salt, out byte[] saltBytes, out string saltBytesDefinition)
				{
					salt = "";
					for (int i = 0; i < 16; i++)
					{
						salt += "abcdefghijklmnopqrstuvwxyz0123456789".Substring(UnityEngine.Random.Range(0, 36), 1);
					}
					saltBytes = Encoding.ASCII.GetBytes(salt);
					saltBytesDefinition = ByteArrayToHexString(saltBytes);
				}

				public static void GeneratePassword(out string password, out byte[] obfuscatedPasswordBytes1, out byte[] obfuscatedPasswordBytes2, out string obfuscatedPasswordBytesDefinition1, out string obfuscatedPasswordBytesDefinition2)
				{
					password = "";
					for (int i = 0; i < 20; i++)
					{
						password += "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".Substring(UnityEngine.Random.Range(0, 62), 1);
					}
					ObfuscatePassword(password, out obfuscatedPasswordBytes1, out obfuscatedPasswordBytes2);
					obfuscatedPasswordBytesDefinition1 = ByteArrayToHexString(obfuscatedPasswordBytes1);
					obfuscatedPasswordBytesDefinition2 = ByteArrayToHexString(obfuscatedPasswordBytes2);
				}

				public static void ObfuscatePassword(string password, out byte[] p1, out byte[] p2)
				{
					p1 = new byte[password.Length];
					p2 = new byte[password.Length];
					for (int i = 0; i < password.Length; i++)
					{
						char c = password[i];
						byte b = (byte)c;
						byte b2 = (byte)UnityEngine.Random.Range(-128, 128);
						p1[i] = b2;
						p2[i] = (byte)(b2 ^ b);
					}
				}

				public static string ByteArrayToHexString(byte[] array)
				{
					string text = "";
					if (array != null)
					{
						for (int i = 0; i < array.Length; i++)
						{
							text += string.Format("0x{0:X2}, ", array[i]);
						}
						if (array.Length != 0)
						{
							text = text.Substring(0, text.Length - 2);
						}
					}
					return text;
				}
			}

			public class SwitchDefinition
			{
				public string mountName = "MySave";

				public long maxFileSizeBytes = 4194304L;

				public SwitchDefinition(string mountName = "MySave", long maxFileSizeBytes = 4194304L)
				{
					this.mountName = mountName;
					this.maxFileSizeBytes = maxFileSizeBytes;
				}

				public SwitchDefinition(SwitchDefinition other)
				{
					mountName = other.mountName;
					maxFileSizeBytes = other.maxFileSizeBytes;
				}
			}

			public class PS4Definition
			{
				public string titleID;

				public string directoryName;

				public string title;

				public string newTitle;

				public string subtitle;

				public string detail;

				public string iconPath;

				public long maxFileSizeBytes;

				public string searchQuery;

				public bool useDialogs;

				public PS4Definition(string titleID = null, string directoryName = null, string title = null, string newTitle = null, string subtitle = null, string detail = null, string iconPath = null, long maxFileSizeBytes = -1L, string searchQuery = null, bool useDialogs = false)
				{
					this.titleID = titleID;
					this.directoryName = directoryName;
					this.title = title;
					this.newTitle = newTitle;
					this.subtitle = subtitle;
					this.detail = detail;
					this.iconPath = iconPath;
					this.maxFileSizeBytes = maxFileSizeBytes;
					this.searchQuery = searchQuery;
					this.useDialogs = useDialogs;
				}

				public PS4Definition(PS4Definition other)
				{
					titleID = other.titleID;
					directoryName = other.directoryName;
					title = other.title;
					newTitle = other.newTitle;
					subtitle = other.subtitle;
					detail = other.detail;
					iconPath = other.iconPath;
					maxFileSizeBytes = other.maxFileSizeBytes;
					searchQuery = other.searchQuery;
					useDialogs = other.useDialogs;
				}
			}

			public class iCloudDefinition
			{
				public bool useCloudKit;

				public bool keepLocalCopy;

				public bool keepLocalCopytvOS;

				public bool cacheLocalCopytvOS;

				public iCloudDefinition(bool useCloudKit = true, bool keepLocalCopy = true, bool keepLocalCopytvOS = false, bool cacheLocalCopytvOS = false)
				{
					this.useCloudKit = useCloudKit;
					this.keepLocalCopy = keepLocalCopy;
					this.keepLocalCopytvOS = keepLocalCopytvOS;
					this.cacheLocalCopytvOS = cacheLocalCopytvOS;
				}

				public iCloudDefinition(iCloudDefinition other)
				{
					useCloudKit = other.useCloudKit;
					keepLocalCopy = other.keepLocalCopy;
					keepLocalCopytvOS = other.keepLocalCopytvOS;
					cacheLocalCopytvOS = other.cacheLocalCopytvOS;
				}
			}

			public string fileName;

			public bool cloudEnabled;

			public bool useEncryption;

			public bool prettyPrint;

			public bool useBinarySerialization;

			public EncryptionDefinition encryptionDefinition = null;

			public SwitchDefinition switchDefinition = null;

			public PS4Definition ps4Definition = null;

			public iCloudDefinition icloudDefinition = null;

			public FileDefinition(string fileName, bool cloudEnabled = false, bool useEncryption = false, bool prettyPrint = false, bool useBinarySerialization = false, EncryptionDefinition encryptionDefinition = null, SwitchDefinition switchDefinition = null, PS4Definition ps4Definition = null, iCloudDefinition icloudDefinition = null)
			{
				this.fileName = fileName;
				this.cloudEnabled = cloudEnabled;
				this.useEncryption = useEncryption;
				this.prettyPrint = prettyPrint;
				this.useBinarySerialization = useBinarySerialization;
				this.encryptionDefinition = encryptionDefinition;
				this.switchDefinition = switchDefinition;
				this.ps4Definition = ps4Definition;
				this.icloudDefinition = icloudDefinition;
			}

			public FileDefinition(FileDefinition other)
			{
				if (other != null)
				{
					fileName = other.fileName;
					cloudEnabled = other.cloudEnabled;
					useEncryption = other.useEncryption;
					prettyPrint = other.prettyPrint;
					useBinarySerialization = other.useBinarySerialization;
					if (other.encryptionDefinition != null)
					{
						encryptionDefinition = new EncryptionDefinition(other.encryptionDefinition);
					}
					if (other.switchDefinition != null)
					{
						switchDefinition = new SwitchDefinition(other.switchDefinition);
					}
					if (other.ps4Definition != null)
					{
						ps4Definition = new PS4Definition(other.ps4Definition);
					}
					if (other.icloudDefinition != null)
					{
						icloudDefinition = new iCloudDefinition(other.icloudDefinition);
					}
				}
			}

			public override bool Equals(object obj)
			{
				FileDefinition fileDefinition = obj as FileDefinition;
				if ((object)fileDefinition == null)
				{
					return false;
				}
				return Equals(fileDefinition);
			}

			public bool Equals(FileDefinition other)
			{
				return other.fileName == fileName;
			}

			public override int GetHashCode()
			{
				return fileName.GetHashCode();
			}

			public static bool operator ==(FileDefinition a, FileDefinition b)
			{
				if ((object)a == b)
				{
					return true;
				}
				if ((object)a == null || (object)b == null)
				{
					return false;
				}
				return a.Equals(b);
			}

			public static bool operator !=(FileDefinition a, FileDefinition b)
			{
				return !(a == b);
			}
		}

		private class FileOperation : Operation
		{
			public OperableFile file { get; private set; }

			public FileOperation(OperableFile file, Action operationType)
			{
				base.complete = false;
				base.result = Result.None;
				this.file = file;
				base.action = operationType;
			}

			public void Execute()
			{
				Log(VerbosityLevel.Info, "UserData:FileOperation:Execute()");
				file.ExecuteOperation(this);
			}

			public override bool Equals(object obj)
			{
				FileOperation fileOperation = obj as FileOperation;
				if ((object)fileOperation == null)
				{
					return false;
				}
				return Equals(fileOperation);
			}

			public bool Equals(FileOperation other)
			{
				return other.action == base.action && file.owner.Equals(other.file.owner) && file.filename.Equals(other.file.filename);
			}

			public override int GetHashCode()
			{
				return base.action.GetHashCode() ^ file.owner.GetHashCode() ^ file.filename.GetHashCode();
			}

			public static bool operator ==(FileOperation a, FileOperation b)
			{
				if ((object)a == b)
				{
					return true;
				}
				if ((object)a == null || (object)b == null)
				{
					return false;
				}
				return a.Equals(b);
			}

			public static bool operator !=(FileOperation a, FileOperation b)
			{
				return !(a == b);
			}
		}

		private class Operation
		{
			public enum Action
			{
				Mount = 0,
				ReadMetadata = 1,
				Read = 2,
				Write = 3,
				Unmount = 4,
				Delete = 5,
				Exists = 6,
				Search = 7
			}

			public Action action { get; protected set; }

			public bool complete { get; protected set; }

			public Result result { get; protected set; }

			public event Action<bool> OnOperationComplete;

			public void AddResult(Result result)
			{
				if (complete)
				{
					throw new InvalidOperationException("Cannot add results to UserDataOperation after completion.");
				}
				this.result |= result;
			}

			public void RemoveResult(Result result)
			{
				if (complete)
				{
					throw new InvalidOperationException("Cannot remove results to UserDataOperation after completion.");
				}
				this.result &= ~result;
			}

			public void Complete()
			{
				result |= Result.Success;
				complete = true;
			}

			public void Fail(Result result)
			{
				if (Result.Success == result)
				{
					throw new ArgumentException("UserDataOperation cannot fail on Result.Success.");
				}
				this.result |= result | Result.Failure;
				complete = true;
			}

			public void NotifyComplete()
			{
				Log(VerbosityLevel.Info, "UserData.UserDataOperation.Complete()");
				if (this.OnOperationComplete != null)
				{
					this.OnOperationComplete(true);
				}
			}
		}

		[Flags]
		public enum Result
		{
			None = 0,
			Success = 1,
			Failure = 2,
			FileNotFound = 4,
			AlreadyMounted = 8,
			MountError = 0x10,
			CorruptData = 0x20,
			NoFreeSpace = 0x40,
			FailEncryption = 0x80,
			CloudUnavailable = 0x100,
			ReconcileFailed = 0x200
		}

		private class UserDataExists
		{
			public void Exists(Profiles.Profile profile, string filename = null, FileDefinition fileDefinition = null)
			{
				FileDefinition fileDefinition2 = new FileDefinition(fileDefinition);
				if (!string.IsNullOrEmpty(filename))
				{
					fileDefinition2.fileName = filename;
				}
				Log(VerbosityLevel.Info, "UserData:UserDataExists.Exists( filename = " + fileDefinition2.fileName + " )");
				UserDataExistsOperationData userDataExistsOperationData = new UserDataExistsOperationData();
				userDataExistsOperationData.profile = profile;
				userDataExistsOperationData.fileDefinition = fileDefinition2;
				UserDataOperation item = new UserDataOperation(userDataExistsOperationData, Operation.Action.Exists);
				if (!pendingOperations.Contains(item))
				{
					pendingOperations.Enqueue(item);
					Log(VerbosityLevel.Info, "UserData:UserDataExists.Exists(): Total queued operations: " + pendingOperations.Count);
					if (currentOperation == null)
					{
						ProcessOperationQueue();
					}
				}
			}

			public void ExecuteExists()
			{
				Operation currentOperation;
				lock (operationLock)
				{
					currentOperation = UserData.currentOperation;
				}
				try
				{
					Log(VerbosityLevel.Info, "UserData:UserDataExists.ExecuteExists()");
					try
					{
						UserDataExistsOperationData operationData = null;
						lock (operationLock)
						{
							UserDataOperation userDataOperation = currentOperation as UserDataOperation;
							if (userDataOperation != null && userDataOperation.operationData != null && userDataOperation.operationData is UserDataExistsOperationData)
							{
								operationData = userDataOperation.operationData as UserDataExistsOperationData;
							}
						}
						ExecuteExists_Platform(operationData);
						lock (operationLock)
						{
							if (currentOperation.complete)
							{
								return;
							}
						}
					}
					catch (Exception ex)
					{
						Log(VerbosityLevel.Error, ex.Message + "\n" + ex.StackTrace);
						lock (operationLock)
						{
							currentOperation.Fail(Result.Failure);
							return;
						}
					}
					lock (operationLock)
					{
						currentOperation.Complete();
					}
				}
				catch (Exception ex2)
				{
					Log(VerbosityLevel.Error, ex2.Message + "\n" + ex2.StackTrace);
				}
			}

			private void ExecuteExists_Platform(UserDataExistsOperationData operationData)
			{
				if (operationData == null || operationData.fileDefinition == null || !operationData.fileDefinition.cloudEnabled)
				{
					Exists_Default(operationData, "");
					return;
				}
				if (Platform.isEditor && !Platform.RunSteamInEditor)
				{
					Exists_Default(operationData, "Steam");
					return;
				}
				if (!Platform.SteamInitialized)
				{
					lock (operationLock)
					{
						currentOperation.Fail(Result.Failure);
						return;
					}
				}
				if (SteamRemoteStorage.FileExists(operationData.fileDefinition.fileName))
				{
					return;
				}
				lock (operationLock)
				{
					currentOperation.Fail(Result.FileNotFound);
				}
			}

			public void NotifyFileExists(object operationData, Result result)
			{
				Log(VerbosityLevel.Info, "UserData:UserDataExists.NotifyFileExists( result = " + result.ToString() + " )");
				if (UserData.OnFileExists != null)
				{
					if (operationData != null && operationData is UserDataExistsOperationData)
					{
						UserDataExistsOperationData userDataExistsOperationData = operationData as UserDataExistsOperationData;
						UserData.OnFileExists(userDataExistsOperationData.profile, userDataExistsOperationData.fileDefinition, result);
					}
					else
					{
						UserData.OnFileExists(null, null, result);
					}
				}
			}

			public void Shutdown()
			{
			}

			private void Exists_Default(UserDataExistsOperationData operationData, string platformDir)
			{
				if (operationData == null || operationData.fileDefinition == null)
				{
					lock (operationLock)
					{
						currentOperation.Fail(Result.FileNotFound);
						return;
					}
				}
				string path = Path.Combine(Path.GetFullPath(PersistentDataPath), platformDir);
				string path2 = Path.Combine(path, operationData.fileDefinition.fileName);
				if (!System.IO.File.Exists(path2))
				{
					lock (operationLock)
					{
						currentOperation.Fail(Result.FileNotFound);
					}
				}
			}
		}

		private class UserDataSearch
		{
			public void Search(Profiles.Profile profile, string searchQuery, FileDefinition fileDefinition = null)
			{
				Log(VerbosityLevel.Info, "UserData:UserDataSearch.Search( searchQuery = " + searchQuery + " )");
				UserDataSearchOperationData userDataSearchOperationData = new UserDataSearchOperationData();
				userDataSearchOperationData.profile = profile;
				userDataSearchOperationData.query = searchQuery;
				userDataSearchOperationData.fileDefinition = fileDefinition;
				UserDataOperation item = new UserDataOperation(userDataSearchOperationData, Operation.Action.Search);
				if (!pendingOperations.Contains(item))
				{
					pendingOperations.Enqueue(item);
					Log(VerbosityLevel.Info, "UserData:UserDataSearch.Search(): Total queued operations: " + pendingOperations.Count);
					if (currentOperation == null)
					{
						ProcessOperationQueue();
					}
				}
			}

			public void ExecuteSearch()
			{
				Operation currentOperation;
				lock (operationLock)
				{
					currentOperation = UserData.currentOperation;
				}
				try
				{
					Log(VerbosityLevel.Info, "UserData:UserDataSearch.ExecuteSearch()");
					try
					{
						UserDataSearchOperationData operationData = null;
						lock (operationLock)
						{
							UserDataOperation userDataOperation = currentOperation as UserDataOperation;
							if (userDataOperation != null && userDataOperation.operationData != null && userDataOperation.operationData is UserDataSearchOperationData)
							{
								operationData = userDataOperation.operationData as UserDataSearchOperationData;
							}
						}
						ExecuteSearch_Platform(operationData);
						lock (operationLock)
						{
							if (currentOperation.complete)
							{
								return;
							}
						}
					}
					catch (Exception ex)
					{
						Log(VerbosityLevel.Error, ex.Message + "\n" + ex.StackTrace);
						lock (operationLock)
						{
							currentOperation.Fail(Result.Failure);
							return;
						}
					}
					lock (operationLock)
					{
						currentOperation.Complete();
					}
				}
				catch (Exception ex2)
				{
					Log(VerbosityLevel.Error, ex2.Message + "\n" + ex2.StackTrace);
				}
			}

			private void ExecuteSearch_Platform(UserDataSearchOperationData operationData)
			{
				if (operationData == null || operationData.fileDefinition == null || !operationData.fileDefinition.cloudEnabled)
				{
					Search_Default(operationData, "");
					return;
				}
				if (Platform.isEditor && !Platform.RunSteamInEditor)
				{
					Search_Default(operationData, "Steam");
					return;
				}
				if (!Platform.SteamInitialized)
				{
					lock (operationLock)
					{
						currentOperation.Fail(Result.Failure);
						return;
					}
				}
				if (!string.IsNullOrEmpty(operationData.query))
				{
					if (operationData.query.Contains("*") || operationData.query.Contains("?"))
					{
						int fileCount = SteamRemoteStorage.GetFileCount();
						string pattern = "^" + Regex.Escape(operationData.query).Replace("\\?", ".").Replace("\\*", ".*") + "$";
						for (int i = 0; i < fileCount; i++)
						{
							int pnFileSizeInBytes;
							string fileNameAndSize = SteamRemoteStorage.GetFileNameAndSize(i, out pnFileSizeInBytes);
							if (!string.IsNullOrEmpty(fileNameAndSize) && Regex.IsMatch(fileNameAndSize, pattern))
							{
								FileDefinition fileDefinition = new FileDefinition(operationData.fileDefinition);
								fileDefinition.fileName = fileNameAndSize;
								SearchResult searchResult = new SearchResult();
								searchResult.fileDefinition = fileDefinition;
								long fileTimestamp = SteamRemoteStorage.GetFileTimestamp(fileNameAndSize);
								searchResult.fileDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
								searchResult.fileDateTime = searchResult.fileDateTime.AddSeconds(fileTimestamp);
								if (operationData.results == null)
								{
									operationData.results = new List<SearchResult>();
								}
								operationData.results.Add(searchResult);
							}
						}
					}
					else if (SteamRemoteStorage.FileExists(operationData.query))
					{
						FileDefinition fileDefinition2 = new FileDefinition(operationData.fileDefinition);
						fileDefinition2.fileName = operationData.query;
						SearchResult searchResult2 = new SearchResult();
						searchResult2.fileDefinition = fileDefinition2;
						long fileTimestamp2 = SteamRemoteStorage.GetFileTimestamp(operationData.query);
						searchResult2.fileDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
						searchResult2.fileDateTime = searchResult2.fileDateTime.AddSeconds(fileTimestamp2);
						if (operationData.results == null)
						{
							operationData.results = new List<SearchResult>();
						}
						operationData.results.Add(searchResult2);
					}
					return;
				}
				lock (operationLock)
				{
					currentOperation.Fail(Result.Failure);
				}
			}

			public void NotifySearchCompleted(object operationData, Result result)
			{
				Log(VerbosityLevel.Info, "UserData:UserDataSearch.NotifySearchCompleted( result = " + result.ToString() + " )");
				if (UserData.OnSearchCompleted != null)
				{
					if (operationData != null && operationData is UserDataSearchOperationData)
					{
						UserDataSearchOperationData userDataSearchOperationData = operationData as UserDataSearchOperationData;
						UserData.OnSearchCompleted(userDataSearchOperationData.profile, userDataSearchOperationData.query, userDataSearchOperationData.results, result);
					}
					else
					{
						UserData.OnSearchCompleted(null, null, null, result);
					}
				}
			}

			public void Shutdown()
			{
			}

			private void Search_Default(UserDataSearchOperationData operationData, string platformDir)
			{
				if (!string.IsNullOrEmpty(operationData.query))
				{
					if (operationData.query.Contains("*") || operationData.query.Contains("?"))
					{
						string path = Path.Combine(Path.GetFullPath(PersistentDataPath), platformDir);
						string[] files = Directory.GetFiles(path, operationData.query, SearchOption.TopDirectoryOnly);
						if (files == null || files.Length == 0)
						{
							return;
						}
						for (int i = 0; i < files.Length; i++)
						{
							FileDefinition fileDefinition = new FileDefinition(operationData.fileDefinition);
							fileDefinition.fileName = Path.GetFileName(files[i]);
							SearchResult searchResult = new SearchResult();
							searchResult.fileDefinition = fileDefinition;
							searchResult.fileDateTime = System.IO.File.GetLastWriteTimeUtc(files[i]);
							if (operationData.results == null)
							{
								operationData.results = new List<SearchResult>();
							}
							operationData.results.Add(searchResult);
						}
						return;
					}
					string path2 = Path.Combine(Path.Combine(Path.GetFullPath(PersistentDataPath), platformDir), operationData.query);
					if (System.IO.File.Exists(path2))
					{
						FileDefinition fileDefinition2 = new FileDefinition(operationData.fileDefinition);
						fileDefinition2.fileName = operationData.query;
						SearchResult searchResult2 = new SearchResult();
						searchResult2.fileDefinition = fileDefinition2;
						searchResult2.fileDateTime = System.IO.File.GetLastWriteTimeUtc(path2);
						if (operationData.results == null)
						{
							operationData.results = new List<SearchResult>();
						}
						operationData.results.Add(searchResult2);
					}
					return;
				}
				lock (operationLock)
				{
					currentOperation.Fail(Result.Failure);
				}
			}
		}

		public delegate void FileExists(Profiles.Profile profile, FileDefinition fileDefinition, Result result);

		private class UserDataExistsOperationData
		{
			public Profiles.Profile profile;

			public FileDefinition fileDefinition;

			public override bool Equals(object obj)
			{
				UserDataExistsOperationData userDataExistsOperationData = obj as UserDataExistsOperationData;
				if ((object)userDataExistsOperationData == null)
				{
					return false;
				}
				return Equals(userDataExistsOperationData);
			}

			public bool Equals(UserDataExistsOperationData other)
			{
				return other.profile == profile && ((fileDefinition == null && other.fileDefinition == null) || (fileDefinition != null && fileDefinition.Equals(other.fileDefinition)));
			}

			public override int GetHashCode()
			{
				if (fileDefinition != null)
				{
					return profile.GetHashCode() ^ fileDefinition.GetHashCode();
				}
				return profile.GetHashCode();
			}

			public static bool operator ==(UserDataExistsOperationData a, UserDataExistsOperationData b)
			{
				if ((object)a == b)
				{
					return true;
				}
				if ((object)a == null || (object)b == null)
				{
					return false;
				}
				return a.Equals(b);
			}

			public static bool operator !=(UserDataExistsOperationData a, UserDataExistsOperationData b)
			{
				return !(a == b);
			}
		}

		private class UserDataOperation : Operation
		{
			public object operationData { get; private set; }

			public UserDataOperation(object operationData, Action operationType)
			{
				base.complete = false;
				base.result = Result.None;
				this.operationData = operationData;
				base.action = operationType;
			}

			public override bool Equals(object obj)
			{
				UserDataOperation userDataOperation = obj as UserDataOperation;
				if ((object)userDataOperation == null)
				{
					return false;
				}
				return Equals(userDataOperation);
			}

			public bool Equals(UserDataOperation other)
			{
				return other.action == base.action && ((operationData == null && other.operationData == null) || (operationData != null && operationData.Equals(other.operationData)));
			}

			public override int GetHashCode()
			{
				if (operationData != null)
				{
					return base.action.GetHashCode() ^ operationData.GetHashCode();
				}
				return base.action.GetHashCode();
			}

			public static bool operator ==(UserDataOperation a, UserDataOperation b)
			{
				if ((object)a == b)
				{
					return true;
				}
				if ((object)a == null || (object)b == null)
				{
					return false;
				}
				return a.Equals(b);
			}

			public static bool operator !=(UserDataOperation a, UserDataOperation b)
			{
				return !(a == b);
			}
		}

		public delegate void SearchCompleted(Profiles.Profile profile, string searchQuery, List<SearchResult> searchResults, Result result);

		public class SearchResult
		{
			public FileDefinition fileDefinition;

			public DateTime fileDateTime;
		}

		private class UserDataSearchOperationData
		{
			public Profiles.Profile profile;

			public string query;

			public FileDefinition fileDefinition;

			public List<SearchResult> results;

			public override bool Equals(object obj)
			{
				UserDataSearchOperationData userDataSearchOperationData = obj as UserDataSearchOperationData;
				if ((object)userDataSearchOperationData == null)
				{
					return false;
				}
				return Equals(userDataSearchOperationData);
			}

			public bool Equals(UserDataSearchOperationData other)
			{
				return other.profile == profile && other.query == query && ((fileDefinition == null && other.fileDefinition == null) || (fileDefinition != null && fileDefinition.Equals(other.fileDefinition)));
			}

			public override int GetHashCode()
			{
				if (fileDefinition != null)
				{
					return profile.GetHashCode() ^ query.GetHashCode() ^ fileDefinition.GetHashCode();
				}
				return profile.GetHashCode() ^ query.GetHashCode();
			}

			public static bool operator ==(UserDataSearchOperationData a, UserDataSearchOperationData b)
			{
				if ((object)a == b)
				{
					return true;
				}
				if ((object)a == null || (object)b == null)
				{
					return false;
				}
				return a.Equals(b);
			}

			public static bool operator !=(UserDataSearchOperationData a, UserDataSearchOperationData b)
			{
				return !(a == b);
			}
		}

		public enum WriteMode
		{
			Immediate = 0,
			Deferred = 1,
			System = 2
		}

		private static Thread userDataOperationThread = null;

		public static VerbosityLevel VerbosityLevel = VerbosityLevel.Info;

		private static string PersistentDataPath;

		private static bool SupportsEncryption = false;

		private static object OperationCriticalSectionReference = new object();

		private static List<Type> RegisteredDataTypes = new List<Type>();

		public static WriteMode SystemWriteMode = WriteMode.Immediate;

		private static Dictionary<Profiles.Profile, Dictionary<string, OperableFile>> MountedUserDataFiles = new Dictionary<Profiles.Profile, Dictionary<string, OperableFile>>();

		private static Operation currentOperation = null;

		private static Queue<Operation> pendingOperations = new Queue<Operation>();

		private static object operationLock = new object();

		private static UserDataExists userDataExists = new UserDataExists();

		private static UserDataSearch userDataSearch = new UserDataSearch();

		public static bool Initialized { get; private set; }

		public static event Action OnInitialized;

		public static event Action<File, Result> OnFileMounted;

		public static event FileExists OnFileExists;

		public static event SearchCompleted OnSearchCompleted;

		public static void Initialize()
		{
			Log(VerbosityLevel.Info, "UserData:Initialize()");
			RegisteredDataTypes.Add(typeof(Hashtable));
			if (Platform.isEditor)
			{
				CheckCompatibilityRequirements();
			}
			Initialize_Platform();
			Initialized = true;
			if (UserData.OnInitialized != null)
			{
				UserData.OnInitialized();
			}
		}

		private static void Initialize_Platform()
		{
			PersistentDataPath = Application.persistentDataPath;
			SupportsEncryption = true;
		}

		public static void Shutdown()
		{
		}

		private static void CheckCompatibilityRequirements()
		{
			string text = Path.Combine(Application.dataPath, "link.xml");
			if (System.IO.File.Exists(text))
			{
				bool flag = false;
				try
				{
					XmlDocument xmlDocument = new XmlDocument();
					xmlDocument.Load(text);
					foreach (XmlNode childNode in xmlDocument.ChildNodes)
					{
						if (childNode.Name == "linker")
						{
							foreach (XmlNode childNode2 in childNode.ChildNodes)
							{
								if (!(childNode2.Name == "assembly"))
								{
									continue;
								}
								bool flag2 = false;
								bool flag3 = false;
								foreach (XmlAttribute attribute in childNode2.Attributes)
								{
									if (attribute.Name == "fullname" && attribute.Value == "System.Runtime.Serialization")
									{
										flag2 = true;
									}
									else if (attribute.Name == "preserve" && attribute.Value == "all")
									{
										flag3 = true;
									}
									if (flag2 && flag3)
									{
										flag = true;
										break;
									}
								}
							}
						}
						if (flag)
						{
							break;
						}
					}
				}
				catch (Exception message)
				{
					Debug.LogError(message);
				}
				if (!flag)
				{
					Debug.LogError("UserData: You must include link.xml with \"<linker><assembly fullname=\"System.Runtime.Serialization\" preserve=\"all\" /></linker>\" in order to have DataContractSerializer to behave correctly in the .NET 4.x scripting runtime on consoles.");
				}
			}
			else
			{
				Debug.LogError("UserData: You must include link.xml with \"<linker><assembly fullname=\"System.Runtime.Serialization\" preserve=\"all\" /></linker>\" in order to have DataContractSerializer to behave correctly in the .NET 4.x scripting runtime on consoles.");
			}
		}

		public static void SetMetadata(object metadata)
		{
		}

		private static void Log(VerbosityLevel verbosityLevel, string message)
		{
			if (VerbosityLevel >= verbosityLevel)
			{
				switch (verbosityLevel)
				{
				case VerbosityLevel.Error:
					Debug.LogError(message);
					break;
				case VerbosityLevel.Warning:
					Debug.LogWarning(message);
					break;
				default:
					Debug.Log(message);
					break;
				}
			}
		}

		public static void RegisterDataType(Type type)
		{
			if (!RegisteredDataTypes.Contains(type))
			{
				RegisteredDataTypes.Add(type);
			}
		}

		private static void ProcessOperationQueue()
		{
			Log(VerbosityLevel.Info, "UserData:ProcessOperationQueue()");
			if (currentOperation != null)
			{
				throw new InvalidOperationException("Operation queue cannot be processed if operations are currently being executed.");
			}
			if (pendingOperations.Count > 0)
			{
				Platform.EnterCriticalSection(OperationCriticalSectionReference);
				currentOperation = pendingOperations.Dequeue();
				currentOperation.OnOperationComplete += CurrentOperation_OnOperationComplete;
				if (currentOperation is FileOperation)
				{
					((FileOperation)currentOperation).Execute();
				}
				else if (currentOperation is UserDataOperation)
				{
					Log(VerbosityLevel.Info, "UserData:ExecuteOperation(): Subscribing to Update");
					Platform.OnUpdate += Platform_Update_UserDataOperation;
					ExecuteUserDataOperation_Platform();
				}
			}
		}

		private static void CurrentOperation_OnOperationComplete(bool success)
		{
			Log(VerbosityLevel.Info, "UserData.CurrentOperation_OnOperationComplete()");
			currentOperation.OnOperationComplete -= CurrentOperation_OnOperationComplete;
			if (Thread.CurrentThread != Platform.mainThread)
			{
				throw new InvalidOperationException("OnOperationComplete event must be fired on the main thread.");
			}
			if (currentOperation is FileOperation)
			{
				FileOperation fileOperation = currentOperation as FileOperation;
				switch (fileOperation.action)
				{
				case Operation.Action.Mount:
				{
					OperableFile file2 = fileOperation.file;
					Result result2 = fileOperation.result;
					if (result2.IsSuccess())
					{
						if (!MountedUserDataFiles.ContainsKey(file2.owner))
						{
							MountedUserDataFiles[file2.owner] = new Dictionary<string, OperableFile>();
						}
						MountedUserDataFiles[file2.owner][file2.filename] = file2;
					}
					Log(VerbosityLevel.Info, "UserData.OnFileMounted( result = " + result2.ToString() + " )");
					if (UserData.OnFileMounted != null)
					{
						UserData.OnFileMounted(file2, result2);
					}
					break;
				}
				case Operation.Action.ReadMetadata:
					fileOperation.file.NotifyReadMetadataCompleted(fileOperation.result);
					break;
				case Operation.Action.Read:
					fileOperation.file.NotifyReadCompleted(fileOperation.result);
					break;
				case Operation.Action.Write:
					fileOperation.file.NotifyWriteCompleted(fileOperation.result);
					break;
				case Operation.Action.Unmount:
				{
					OperableFile file = fileOperation.file;
					Result result = fileOperation.result;
					if (result.IsSuccess() && MountedUserDataFiles.ContainsKey(file.owner))
					{
						MountedUserDataFiles[file.owner].Remove(file.filename);
					}
					file.NotifyUnmountCompleted(result);
					break;
				}
				case Operation.Action.Delete:
					fileOperation.file.NotifyDeleteCompleted(fileOperation.result);
					break;
				}
			}
			else if (currentOperation is UserDataOperation)
			{
				UserDataOperation userDataOperation = currentOperation as UserDataOperation;
				switch (userDataOperation.action)
				{
				case Operation.Action.Exists:
					userDataExists.NotifyFileExists(userDataOperation.operationData, userDataOperation.result);
					break;
				case Operation.Action.Search:
					userDataSearch.NotifySearchCompleted(userDataOperation.operationData, userDataOperation.result);
					break;
				}
			}
			currentOperation = null;
			if (pendingOperations.Count > 0)
			{
				ProcessOperationQueue();
			}
			else
			{
				Platform.ExitCriticalSection(OperationCriticalSectionReference);
			}
		}

		public static void Mount(Profiles.Profile profile, string filename = null, FileDefinition fileDefinition = null)
		{
			if (string.IsNullOrEmpty(filename))
			{
				if (fileDefinition == null || string.IsNullOrEmpty(fileDefinition.fileName))
				{
					Log(VerbosityLevel.Error, "UserData.Mount: No name provided for file!");
					if (UserData.OnFileMounted != null)
					{
						UserData.OnFileMounted(null, Result.Failure);
					}
					return;
				}
				if (fileDefinition != null)
				{
					filename = fileDefinition.fileName;
				}
			}
			else if (fileDefinition != null && !string.IsNullOrEmpty(fileDefinition.fileName) && fileDefinition.fileName != filename)
			{
				Log(VerbosityLevel.Warning, "UserData.Mount: filename [" + filename + "] does not match FileDefinition filename [" + fileDefinition.fileName + "]!");
			}
			Log(VerbosityLevel.Info, "UserData:Mount( filename = " + filename + " )");
			if (MountedUserDataFiles.ContainsKey(profile) && MountedUserDataFiles[profile].ContainsKey(filename))
			{
				OperableFile operableFile = MountedUserDataFiles[profile][filename];
				if (operableFile.unmountInProgress)
				{
					Debug.LogError("Attempted to mount file that is being unmounted: " + filename);
					if (UserData.OnFileMounted != null)
					{
						UserData.OnFileMounted(operableFile, Result.Failure | Result.MountError);
					}
				}
				else
				{
					Debug.LogWarning("Attempted to mount file already mounted: " + filename);
					if (UserData.OnFileMounted != null)
					{
						UserData.OnFileMounted(operableFile, Result.Success | Result.AlreadyMounted);
					}
				}
			}
			OperableFile operableFile2 = new OperableFile();
			operableFile2.filename = filename;
			operableFile2.owner = profile;
			operableFile2.fileDateTime = DateTime.UtcNow;
			operableFile2.LoadFileDefinition(fileDefinition);
			FileOperation fileOperation = new FileOperation(operableFile2, Operation.Action.Mount);
			if ((currentOperation == null || !currentOperation.Equals(fileOperation)) && !pendingOperations.Contains(fileOperation))
			{
				pendingOperations.Enqueue(fileOperation);
				Log(VerbosityLevel.Info, "UserData.Mount(): Total queued operations: " + pendingOperations.Count);
				if (currentOperation == null)
				{
					ProcessOperationQueue();
				}
			}
		}

		private static void RequestReadMetadata(OperableFile file)
		{
			Log(VerbosityLevel.Info, "UserData:RequestReadMetadata( filename = " + file.filename + " )");
			FileOperation item = new FileOperation(file, Operation.Action.ReadMetadata);
			if (!pendingOperations.Contains(item))
			{
				pendingOperations.Enqueue(item);
				Log(VerbosityLevel.Info, "UserData.RequestReadMetadata(): Total queued operations: " + pendingOperations.Count);
				if (currentOperation == null)
				{
					ProcessOperationQueue();
				}
			}
		}

		private static void RequestRead(OperableFile file)
		{
			Log(VerbosityLevel.Info, "UserData:RequestRead( filename = " + file.filename + " )");
			FileOperation item = new FileOperation(file, Operation.Action.Read);
			if (!pendingOperations.Contains(item))
			{
				pendingOperations.Enqueue(item);
				Log(VerbosityLevel.Info, "UserData.RequestRead(): Total queued operations: " + pendingOperations.Count);
				if (currentOperation == null)
				{
					ProcessOperationQueue();
				}
			}
		}

		private static void RequestWrite(OperableFile file)
		{
			Log(VerbosityLevel.Info, "UserData:RequestWrite( filename = " + file.filename + " )");
			FileOperation item = new FileOperation(file, Operation.Action.Write);
			if (!pendingOperations.Contains(item))
			{
				pendingOperations.Enqueue(item);
				Log(VerbosityLevel.Info, "UserData.RequestWrite(): Total queued operations: " + pendingOperations.Count);
				if (currentOperation == null)
				{
					ProcessOperationQueue();
				}
			}
		}

		private static void RequestUnmount(OperableFile file)
		{
			Log(VerbosityLevel.Info, "UserData:RequestUnmount( filename = " + file.filename + " )");
			FileOperation item = new FileOperation(file, Operation.Action.Unmount);
			if (!pendingOperations.Contains(item))
			{
				pendingOperations.Enqueue(item);
				Log(VerbosityLevel.Info, "UserData.RequestUnmount(): Total queued operations: " + pendingOperations.Count);
				if (currentOperation == null)
				{
					ProcessOperationQueue();
				}
			}
		}

		private static void RequestDelete(OperableFile file)
		{
			Log(VerbosityLevel.Info, "UserData:RequestDelete( filename = " + file.filename + " )");
			FileOperation item = new FileOperation(file, Operation.Action.Delete);
			if (!pendingOperations.Contains(item))
			{
				pendingOperations.Enqueue(item);
				Log(VerbosityLevel.Info, "UserData.RequestDelete(): Total queued operations: " + pendingOperations.Count);
				if (currentOperation == null)
				{
					ProcessOperationQueue();
				}
			}
		}

		private static void ExecuteUserDataOperation_Platform()
		{
			Log(VerbosityLevel.Info, "UserData.OperableFile.Execute_Platform()");
			lock (operationLock)
			{
				if (currentOperation == null)
				{
					throw new InvalidOperationException("ExecuteUserDataOperation_Platform cannot be called if currentUserDataOperation is null.");
				}
				switch (currentOperation.action)
				{
				case Operation.Action.Exists:
					userDataOperationThread = new Thread(ExecuteExists);
					userDataOperationThread.Start();
					Log(VerbosityLevel.Info, "UserData.ExecuteUserDataOperation_Platform(): Exists operation started.");
					break;
				case Operation.Action.Search:
					userDataOperationThread = new Thread(ExecuteSearch);
					userDataOperationThread.Start();
					Log(VerbosityLevel.Info, "UserData.ExecuteUserDataOperation_Platform(): Search operation started.");
					break;
				}
			}
		}

		private static void Platform_Update_UserDataOperation()
		{
			lock (operationLock)
			{
				if (currentOperation.complete)
				{
					Log(VerbosityLevel.Info, "UserData.Platform_Update_UserDataOperation(): Operation complete.");
					Log(VerbosityLevel.Info, "UserData:Platform_Update_UserDataOperation(): Unsubscribing from Update");
					Platform.OnUpdate -= Platform_Update_UserDataOperation;
					CompleteUserDataOperation_Platform(currentOperation as UserDataOperation);
					currentOperation.NotifyComplete();
				}
			}
		}

		private static void CompleteUserDataOperation_Platform(UserDataOperation completedOperation)
		{
			Log(VerbosityLevel.Info, "UserData.CompleteUserDataOperation_Platform()");
		}

		public static void Exists(Profiles.Profile profile, string filename = null, FileDefinition fileDefinition = null)
		{
			userDataExists.Exists(profile, filename, fileDefinition);
		}

		private static void ExecuteExists()
		{
			userDataExists.ExecuteExists();
		}

		public static void Search(Profiles.Profile profile, string searchQuery, FileDefinition fileDefinition = null)
		{
			userDataSearch.Search(profile, searchQuery, fileDefinition);
		}

		private static void ExecuteSearch()
		{
			userDataSearch.ExecuteSearch();
		}
	}
}
