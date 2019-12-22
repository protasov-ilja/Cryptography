using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Caesar
{
    public class CaesarEncoder
    {
        private readonly static string _alphabet = "\n\r\"\',.:;!-()0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZАБВГДЕЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";

        public static readonly Encoding Encoding = Encoding.UTF8;

        public static int MaxKeyLength = 20;

        #region Encoding
        public static byte[] Encode( string inputBytes, string key )
        {
            var filtered = CheckInput( inputBytes );

            byte[] filteredBytes = Encoding.GetBytes( filtered );
            byte[] keyBytes = Encoding.GetBytes( key );

            return EncodeData( filteredBytes, keyBytes );
        }

        private static byte[] EncodeData( byte[] inputBytes, byte[] key )
        {
            return inputBytes.Zip( GetBytePerCircle( key ), ( byte originalByte, byte keyByte ) =>
            {
                return ( byte )( ( originalByte + keyByte ) % 256 );
            } ).ToArray();
        }
        #endregion

        #region Decoding
        private static byte[] DecodeData( byte[] inputBytes, byte[] key )
        {
            return inputBytes.Zip( GetBytePerCircle( key ), ( byte originalByte, byte keyByte ) =>
            {
                int difference = originalByte - keyByte;
                if ( difference < 0 )
                {
                    difference = 256 + difference;
                }

                return ( byte )difference;
            } ).ToArray();
        }

        public static byte[] Decode( byte[] inputBytes, string key )
        {
            byte[] keyBytes = Encoding.GetBytes( key );

            return DecodeData( inputBytes, keyBytes );
        }
        #endregion

        #region Hacking
        public static List<byte[]> GetKeys( byte[] encryptedBytes )
        {
            var keys = new List<byte[]>();
            for ( int keyLength = 1; keyLength <= MaxKeyLength; ++keyLength ) // found all keys
            {
                var keySet = new List<HashSet<byte>>();
                var isKeysNotFound = false;

                for ( int keyIndex = 0; keyIndex < keyLength; ++keyIndex ) // found key
                {
                    var shiftSet = new HashSet<byte>();
                    for ( int i = keyIndex; i < encryptedBytes.Length; i += keyLength )
                    {
                        if ( i == keyIndex )
                        {
                            shiftSet.UnionWith( GetPossibleShifts( encryptedBytes[ i ] ) );
                        }
                        else
                        {
                            shiftSet.IntersectWith( GetPossibleShifts( encryptedBytes[ i ] ) );
                        }
                    }

                    if ( shiftSet.Count == 0 )
                    {
                        isKeysNotFound = true;
                        break;
                    }

                    keySet.Add( shiftSet );
                }

                if ( !isKeysNotFound )
                {
                    keys = new List<byte[]>();
                    var stack = new Stack<ByteData>();
                    stack.Push( new ByteData { Bytes = new byte[ keyLength ], Length = 0 } );

                    while ( stack.Count != 0 )
                    {
                        ByteData data = stack.Pop();
                        if ( data.Length == keyLength )
                        {
                            keys.Add( data.Bytes );
                        }
                        else
                        {
                            foreach ( byte possibleShift in keySet[ data.Length ] ) // collect all possible keys
                            {
                                byte[] key = new byte[ keyLength ]; // create key
                                Array.Copy( data.Bytes, key, keyLength ); // copy with current key
                                key[ data.Length ] = possibleShift;
                                stack.Push( new ByteData { Bytes = key, Length = data.Length + 1 } ); 
                            }
                        }
                    }

                    return keys;
                }
            }

            return new List<byte[]>();
        }

        private static HashSet<byte> GetPossibleShifts( byte encrypted )
        {
            var set = new HashSet<byte>();
            foreach ( byte letter in _alphabet )
            {
                int key = encrypted - letter;
                if ( key < 0 )
                {
                    key = 256 + key;
                }
                set.Add( ( byte )key );
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
                if ( _alphabet.IndexOf( ch ) != -1 )
                {
                    builder.Append( ch );
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
