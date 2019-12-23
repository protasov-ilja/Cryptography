using System;
using System.IO;
using System.Linq;
using CommandLine;

namespace Caesar
{
    public class Program
    {
        static void Main( string[] args )
        {
            try
            {
                var i = Parser.Default.ParseArguments<EncodeOptions, DecodeOptions, HackKeyOptions>( args )
                .MapResult(
                    ( EncodeOptions options ) => RunEncode( options ),
                    ( DecodeOptions options ) => RunDecode( options ),
                    ( HackKeyOptions options ) => RunHack( options ),
                    errors => throw new Exception( "Parsing Error!" ) );
            }
            catch ( Exception e )
            {
                Console.WriteLine( e.Message );
            }
        }

        private static int RunEncode( EncodeOptions options )
        {
            Console.WriteLine( "Encoding..." );
            string input = File.ReadAllText( options.InputFile, CaesarEncoder.Encoding );
            byte[] encoded = CaesarEncoder.Encode( input, options.Key );
            File.WriteAllBytes( options.OutputFile, encoded );
            Console.WriteLine( "Encoding: SUCCESS" );

            return 0;
        }

        private static int RunDecode( DecodeOptions options )
        {
            Console.WriteLine( "Decoding..." );
            byte[] input = File.ReadAllBytes( options.InputFile );
            byte[] decoded = CaesarEncoder.Decode( input, options.Key );
            File.WriteAllBytes( options.OutputFile, decoded );
            Console.WriteLine( "Decoding: SUCCESS" );

            return 0;
        }

        private static int RunHack( HackKeyOptions options )
        {
            Console.WriteLine( "Hacking..." );
            byte[] input = File.ReadAllBytes( options.InputFile );
            var keys = CaesarEncoder.GetKeys( input );
            if ( keys.Any() )
            {
                Console.WriteLine( "Keys found:" );
                using ( var fileStream = new FileStream( options.OutputFile, FileMode.Create ) )
                using ( var streamWriter = new StreamWriter( fileStream, CaesarEncoder.Encoding ) )
                {
                    System.Collections.IList list = keys;
                    for ( int i = 0; i < list.Count; i++ )
                    {
                        string key = ( string )list[ i ];
                        streamWriter.WriteLine( key );
                    }
                }
            }


            //    if ( keys.Count != 0 )
            //{
            //    Console.WriteLine( "Hacking: SUCCESS" );
            //    Console.WriteLine( "Keys found:" );
            //    foreach ( byte[] key in keys )
            //    {
            //        Console.WriteLine( CaesarEncoder.Encoding.GetString( key ) );
            //    }
            //}
            else
            {
                Console.WriteLine( "No keys found!" );
            }

            return 0;
        }
    }
}
