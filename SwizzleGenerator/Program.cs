using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SwizzleGenerator
{
    public class VectorData
    {
        public string Name;
        public string ReturnTypeName;
        public string DefaultTypeName;
        public string SwizzleChars;
        public bool IsDefaultAvailable;
        public int MaxOut;
        public string ExistingAccessor;
        public VectorData(string name, string returnName, int maxOut, string defaultTypeName, string swizzle, string existingAcc, bool isDefaultAvailable = false)
        {
            Name = name;
            MaxOut = maxOut;
            ReturnTypeName = returnName;
            DefaultTypeName = defaultTypeName;
            SwizzleChars = swizzle;
            IsDefaultAvailable = isDefaultAvailable;
            ExistingAccessor = existingAcc;
        }
    }

    class Program
    {
        public static bool NoDuplicatedChar(string str)
        {
            HashSet<char> containedChars = new HashSet<char>();

            var chars = str.ToCharArray();
            foreach (var c in chars)
            {
                if (containedChars.Contains(c))
                    return false;
                containedChars.Add(c);
            }
            return true;
        }

        public static bool NextPermutation(int[] permutation, int highestValue)
        {
            permutation[0] += 1;
            for (int i = 0; i < permutation.Length; ++i)
            {
                if (permutation[i] > highestValue)
                {
                    permutation[i] = 0;
                    if (i < (permutation.Length - 1))
                        permutation[i + 1] += 1;
                    else
                        return false;
                }
            }
            return true;
        }

        private static readonly string SUFFIX = "SwizzleExtension";
        [STAThread]
        static void Main(string[] args)
        {
            VectorData[] vectors = new VectorData[] {
                // Float ==========================================
                // XYZW
                new VectorData("Vector2", "Vector{0}",4, "float", "xy", "xy", true),
                new VectorData("Vector3", "Vector{0}",4, "float",  "xyz","xyz", true),
                new VectorData("Vector4", "Vector{0}",4, "float",  "xyzw", "xyzw",true),
                // RGBA
                new VectorData("Vector2", "Vector{0}",4, "float",  "rg", "xy"),
                new VectorData("Vector3", "Vector{0}",4, "float",  "rgb", "xyz"),
                new VectorData("Vector4", "Vector{0}",4, "float",  "rgba", "xyzw"),
                // STPQ
                new VectorData("Vector2", "Vector{0}",4, "float",  "st", "xy"),
                new VectorData("Vector3", "Vector{0}",4, "float",  "stp", "xyz"),
                new VectorData("Vector4", "Vector{0}",4, "float",  "stpq", "xyzw"),

                // Int ==========================================
                // XYZW
                new VectorData("Vector2Int", "Vector{0}Int",4, "int",  "xy", "xy", true),
                new VectorData("Vector3Int", "Vector{0}Int",4, "int",  "xyz", "xyz", true),
                new VectorData("Vector4Int", "Vector{0}Int",4, "int",  "xyzw", "xyzw"),
                // RGBA
                new VectorData("Vector2Int", "Vector{0}Int",4, "int", "rg", "xy"),
                new VectorData("Vector3Int", "Vector{0}Int",4, "int", "rgb", "xyz"),
                new VectorData("Vector4Int", "Vector{0}Int",4, "int", "rgba", "xyzw"),
                // STPQ
                new VectorData("Vector2Int", "Vector{0}Int",4, "int", "st", "xy"),
                new VectorData("Vector3Int", "Vector{0}Int",4, "int", "stp", "xyz"),
                new VectorData("Vector4Int", "Vector{0}Int",4, "int", "stpq", "xyzw"),

                // Color ==========================================
                new VectorData("Color", "Vector{0}",4, "float", "xyzw", "rgba"),
                new VectorData("Color", "Vector{0}",4, "float", "rgba", "rgba"),
                new VectorData("Color", "Vector{0}",4, "float", "stpq", "rgba"),

                // Color32 ==========================================
                new VectorData("Color32", "Vector{0}Int",4, "byte",  "xyzw",  "rgba"),
                new VectorData("Color32", "Vector{0}Int",4, "byte",  "rgba",  "rgba"),
                new VectorData("Color32", "Vector{0}Int",4, "byte",  "stpq",  "rgba"),
            };
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("using UnityEngine;");
            sb.AppendLine();

            sb.AppendLine("// This code is generated");
            sb.AppendLine("// The goal is to provide shortcuts for accessing vector components like in shader languages (feature called swizzle).");
            sb.AppendLine("// This makes possible to use a vector this way :");
            sb.AppendLine("// Vector3 v; v.xzxz();");
            sb.AppendLine("// Vector2 v; v.yyyy();");
            sb.AppendLine("// Vector4 v; v.ww();");
            sb.AppendLine("// ...");
            sb.AppendLine();

            sb.AppendLine("namespace UnitySwizzle");
            sb.AppendLine("{");
            foreach (var vectorData in vectors)
            {
                sb.AppendLine($"\tpublic static class {vectorData.Name + vectorData.SwizzleChars + SUFFIX}");
                sb.AppendLine("\t{");
                for (int i = 1; i < (vectorData.MaxOut + 1); ++i)
                {
                    sb.AppendLine($"\t\t// {vectorData.Name} with {i} components.");

                    int[] permuts = new int[i];
                    bool checkNext = true;
                    while (checkNext)
                    {

                        string swizzleStr = "";
                        string swizzleAccess = "";
                        string swizzleSet = "";
                        for (int j = 0; j < permuts.Length; ++j)
                        {
                            var swizzleChar = vectorData.SwizzleChars[permuts[j]];
                            swizzleStr += swizzleChar;
                            var accessSwizzle = vectorData.ExistingAccessor[permuts[j]];
                            swizzleAccess += $"v.{accessSwizzle}";
                            
                            try
                            {

                                swizzleSet += $"v.{accessSwizzle} = ";
                                if (i == 1)
                                    swizzleSet += "other;";
                                else
                                    swizzleSet += $"({vectorData.DefaultTypeName})other.{vectorData.ExistingAccessor[j]}();";
                            }
                            catch (Exception _) { }
                            //if (!vectorData.IsDefaultAvailable)
                            //    swizzleAccess += "()";
                            if (j < (permuts.Length - 1))
                            {
                                swizzleAccess += ", ";
                            }
                        }
                        string returnName = string.Format(vectorData.ReturnTypeName, i);

                        string ctr = $"new {returnName}({swizzleAccess})";
                        if (i == 1)
                        {
                            ctr = $"{swizzleAccess}";
                            returnName = vectorData.DefaultTypeName;
                        }

                        sb.AppendLine($"\t\tpublic static {returnName} {swizzleStr}(this {vectorData.Name} v) {{ return {ctr}; }}");

                        if (NoDuplicatedChar(swizzleStr))
                            sb.AppendLine($"\t\tpublic static {returnName} {swizzleStr}(this {vectorData.Name} v, {returnName} other) {{ {swizzleSet} return v.{swizzleStr}(); }}");

                        checkNext = NextPermutation(permuts, vectorData.SwizzleChars.Length - 1);
                    }
                    if (i < 4)
                        sb.AppendLine();
                }
                sb.AppendLine("\t}");
                if (vectorData != vectors.Last())
                    sb.AppendLine();


            }
            sb.AppendLine("}");
            var text = sb.ToString();

            SaveFileDialog sfd = new SaveFileDialog();
            var res = sfd.ShowDialog();
            if (res == DialogResult.OK)
            {
                File.WriteAllText(sfd.FileName, text);
            }

            Console.WriteLine(text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Length + " lines of code generated");
            Console.Read();
        }
    }
}
