using Microsoft.Data.Sqlite;
using System.Diagnostics;
using System.Reflection;

namespace Squidward{
    public class Program{
        static Random rd = new Random();
        public static void Main(string[] args){ // arg1 = color data file (first line is palette name, other lines alternate color name - hex)

            string[] colorsLines = File.ReadAllLines(args[0]);

            string paletteName = colorsLines[0];

            if(args.Length < 1){
                return;
            }

            string dbid =
            CreateUUID(8) + @"-" +
            CreateUUID(4) + @"-" +
            CreateUUID(4) + @"-" +
            CreateUUID(4) + @"-" +
            CreateUUID(12);

            File.Copy(AppDomain.CurrentDomain.BaseDirectory+"schema.calib", paletteName+".calib");
            
            using (var connection = new SqliteConnection("Data Source="+paletteName+".calib"))
            {

                connection.Open();

                SqliteCommand addGlobalData = connection.CreateCommand();
                addGlobalData.CommandText = @"
                    INSERT INTO GlobalData (GlobalId, CategoryId, Lock, ConvertedAlbs, DatabaseUniqueId)
                    VALUES (1, 0, 0, 0, '"+dbid+@"');
                ";

                addGlobalData.ExecuteNonQuery();
                addGlobalData.Dispose();

                uint numColors = 0;

                for(int i = 1; i < colorsLines.Length; i+=2){
                    numColors++;
                    string name = colorsLines[i];
                    string hex = colorsLines[i+1];
                    AddColor(name, dbid, hex, numColors, connection);
                }

                string itemsXml = "";
                for(int i = 1; i <= numColors; i++){
                    itemsXml += @"<Item Id="""+i.ToString()+@"""/>";                    
                }

                SqliteCommand addColorViews = connection.CreateCommand();
                addColorViews.CommandText = @"
                    INSERT INTO LibraryViews (LibraryViewId, LibraryView, Name)
                    VALUES (1, '''<?xml version=""1.0"" encoding=""UTF-8"" standalone=""no"" ?><TreeView xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:noNamespaceSchemaLocation=""LibraryDatabaseTreeView.xsd""><Product><Name>Chief Architect Premier X13 Academic Version</Name><ProductVersion>23.3.0.81</ProductVersion><FileVersion>3256</FileVersion></Product><Lock>0</Lock><Database Id=""{"+dbid+@"}"" Source=""%temp%/ChiefTemp__2.calib""/><Directory Name="""+paletteName+@""">"+itemsXml+@"</Directory></TreeView>''', 'User Catalog');
                ";
                addColorViews.ExecuteNonQuery();
                addColorViews.Dispose();

                connection.Close();
                connection.Dispose();
            }
            SqliteConnection.ClearAllPools();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if(WaitForFile(paletteName+".calib")){
                var p = new Process();
                p.StartInfo = new ProcessStartInfo(Path.GetFullPath(paletteName+".calib"))
                { 
                    UseShellExecute = true
                };
                p.Start();
            }
            else{
                Console.WriteLine("File Locked!");
                Thread.Sleep(50);
            }
        }
        private static void AddColor(string colorName, string DatabaseUniqueId, string rgbhex, uint id, SqliteConnection connection){

                string ColorObjUniqueId =
                CreateUUID(8) + @"-" +
                CreateUUID(4) + @"-" +
                CreateUUID(4) + @"-" +
                CreateUUID(4) + @"-" +
                CreateUUID(12);
                
                string colorNameHex = "";
                foreach(char c in colorName){
                    colorNameHex += BitConverter.ToString(new byte[]{(byte)c});                    
                }
                string colorNameLenHex = BitConverter.ToString(new byte[]{BitConverter.GetBytes(colorName.Length)[0]});

                SqliteCommand insertData = connection.CreateCommand();
                insertData.CommandText = @"
                    INSERT INTO AssociatedData (AssociatedDataId, AssociatedDataBlob)
                    VALUES ("+id+@", X'000000000000000000000000140000007b0a202020202256657273696f6e223a20330a7d0000000000000000000000000000000000');
                    INSERT INTO Data4LibraryObjects (LibraryObjectId, Data)
                    VALUES ("+id+@", X'cdab3900000000000000000000000000000000000000000000000000000000000000000000000000b89b49f36bb45e448fb89d47295ee081"+colorNameLenHex+@"000000"+colorNameHex+@"0000000000"+rgbhex+@"ff0000000000000000000000000000000000000000000000344000000000000034401f0000000000000000000040e17a843f0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001cdab320106000000000000000000803f0000803f0000803f010000009a99993e9a99993e9a99993e0300000000000000000000000000000004000000cdcccc3ecdcccc3ecdcccc3e0a0000000000000000000000000000000c00000000000000000000000000000005000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001000000ff00000000000000000000000000000000000000000000000000f03f00ffffffff02000000000000e03f00');
                    INSERT INTO LibraryObjects (LibraryObjectId, Name, Type, Lock, CopyrightId, Metric, ElementVersion, LibSymDataId, PlantDataId, ProductFilterId, CreationTime, ModificationTime, UniqueId, ShortcutDatabaseId, ImportName)
                    VALUES ("+id+@", '"+colorName+@"', 8, -1, NULL, 0, 3256, NULL, NULL, 1, '', '','"+ColorObjUniqueId+@"', NULL, '"+colorName+@"');
                    INSERT INTO SymbolData4LibraryObjects (LibraryObjectId, SymbolData)
                    VALUES ("+id+@", NULL);
                ";

                insertData.ExecuteNonQuery();
        }

        internal static string CreateUUID(int stringLength)
        {
            const string allowedChars = "abcdef0123456789";
            char[] chars = new char[stringLength];

            for (int i = 0; i < stringLength; i++)
            {
                chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
            }

            return new string(chars);
        }

        private static bool WaitForFile (string fullPath) // https://stackoverflow.com/a/3677960
        {
            for (int numTries = 0; numTries < 20; numTries++) {
                try {
                    new FileStream (fullPath, FileMode.Open).Dispose();
                    return true;
                }
                catch (IOException) {
                    Thread.Sleep (50);
                }
            }

            return false;
        }
    }
}
