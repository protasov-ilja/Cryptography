using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Caesar
{
	public class CaesarEncoder
	{
		private static string _alphabet = "\n\r\"\',.:;!-()0123456789abcdefghijklmnopqrstuvwxyzабвгдежзийклмнопрстуфхцчшщъыьэюя";

		public static readonly Encoding Encoding = Encoding.UTF8;

		public static int MaxKeyLength = 20;

		#region Encoding
		public static byte[] Encode( string message, string key )
		{
			string filtered = CheckInput( message );

			byte[] filteredBytes = Encoding.GetBytes( filtered );
			byte[] keyBytes = Encoding.GetBytes( key );

			return Encode( filteredBytes, keyBytes );
		}

		private static byte[] Encode( byte[] message, byte[] key )
		{
			return message.Zip( GetBytePerCircle( key ), ( byte originalByte, byte keyByte ) =>
			{
				return ( byte )( ( originalByte + keyByte ) % 256 );
			}).ToArray();
		}
		#endregion

		#region Decoding
		private static byte[] Decode( byte[] message, byte[] key )
		{
			return message.Zip( GetBytePerCircle( key ), ( byte originalByte, byte keyByte ) =>
			{
				int difference = originalByte - keyByte;
				if ( difference < 0 )
				{
					difference = 256 + difference;
				}

				return ( byte ) difference;
			}).ToArray();
		}

		public static byte[] Decode(byte[] message, string key)
		{
			byte[] keyBytes = Encoding.GetBytes(key);
			return Decode(message, keyBytes);
		}
		#endregion

		#region Hacking
		public static bool TryHackKey(byte[] message, out List<byte[]> keys)
		{
			for (int keyLength = 1; keyLength <= MaxKeyLength; ++keyLength)
			{
				var keyLetters = new List<HashSet<byte>>();
				bool failed = false;

				for (int keyIndex = 0; keyIndex < keyLength; ++keyIndex)
				{
					var key = new HashSet<byte>();
					for (int i = keyIndex; i < message.Length; i += keyLength)
					{
						if (i == keyIndex)
						{
							key.UnionWith(GetPossibleKeys(message[i]));
						}
						else
						{
							key.IntersectWith(GetPossibleKeys(message[i]));
						}
					}

					if (key.Count == 0)
					{
						failed = true;
						break;
					}

					keyLetters.Add(key);
				}

				if (!failed)
				{
					keys = new List<byte[]>();
					var stack = new Stack<ByteData>();
					stack.Push(new ByteData { Bytes = new byte[keyLength], Length = 0 });

					while (stack.Count != 0)
					{
						ByteData data = stack.Pop();
						if (data.Length == keyLength)
						{
							keys.Add(data.Bytes);
						}
						else
						{
							foreach (byte keyLetter in keyLetters[data.Length])
							{
								byte[] key = new byte[keyLength];
								Array.Copy(data.Bytes, key, keyLength);
								key[data.Length] = keyLetter;
								stack.Push(new ByteData { Bytes = key, Length = data.Length + 1 });
							}
						}
					}

					return true;
				}
			}

			keys = null;
			return false;
		}

		private static HashSet<byte> GetPossibleKeys(byte encrypted)
		{
			var set = new HashSet<byte>();
			foreach (byte letter in _alphabet)
			{
				int key = encrypted - letter;
				if (key < 0)
				{
					key = 256 + key;
				}
				set.Add((byte)key);
			}

			return set;
		}

		private struct ByteData
		{
			public byte[] Bytes;
			public int Length;
		}
		#endregion

		private static string CheckInput( string input )
		{
			var builder = new StringBuilder();
			foreach ( char ch in input )
			{
				char lower = char.ToLowerInvariant( ch );
				if ( _alphabet.IndexOf( lower ) != -1 )
				{
					builder.Append( lower );
				}
				else
				{
					throw new Exception( $"Simbol not in alphabet { ch }" );
				}
			}

			return builder.ToString();
		}

		private static IEnumerable<byte> GetBytePerCircle( byte[] bytes )
		{
			while ( true )
			{
				foreach ( byte b in bytes )
				{
					yield return b;
				}
			}
		}
	}
}
