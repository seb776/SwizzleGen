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
                new VectorData("vec2", "vec{0}",4, "float", "xy", "xy", true),
                new VectorData("vec3", "vec{0}",4, "float",  "xyz","xyz", true),
                new VectorData("vec4", "vec{0}",4, "float",  "xyzw", "xyzw",true),
                // RGBA
                new VectorData("vec2", "vec{0}",4, "float",  "rg", "xy"),
                new VectorData("vec3", "vec{0}",4, "float",  "rgb", "xyz"),
                new VectorData("vec4", "vec{0}",4, "float",  "rgba", "xyzw"),
                // STPQ
                new VectorData("vec2", "vec{0}",4, "float",  "st", "xy"),
                new VectorData("vec3", "vec{0}",4, "float",  "stp", "xyz"),
                new VectorData("vec4", "vec{0}",4, "float",  "stpq", "xyzw"),

                // Int ==========================================
                // XYZW
                new VectorData("ivec2", "ivec{0}",4, "int",  "xy", "xy", true),
                new VectorData("ivec3", "ivec{0}",4, "int",  "xyz", "xyz", true),
                new VectorData("ivec4", "ivec{0}",4, "int",  "xyzw", "xyzw"),
                // RGBA
                new VectorData("ivec2", "ivec{0}",4, "int", "rg", "xy"),
                new VectorData("ivec3", "ivec{0}",4, "int", "rgb", "xyz"),
                new VectorData("ivec4", "ivec{0}",4, "int", "rgba", "xyzw"),
                // STPQ
                new VectorData("ivec2", "ivec{0}",4, "int", "st", "xy"),
                new VectorData("ivec3", "ivec{0}",4, "int", "stp", "xyz"),
                new VectorData("ivec4", "ivec{0}",4, "int", "stpq", "xyzw"),

                // UInt ==========================================
                // XYZW
                new VectorData("uvec2", "uvec{0}",4, "int",  "xy", "xy", true),
                new VectorData("uvec3", "uvec{0}",4, "int",  "xyz", "xyz", true),
                new VectorData("uvec4", "uvec{0}",4, "int",  "xyzw", "xyzw"),
                // RGBA
                new VectorData("uvec2", "uvec{0}",4, "int", "rg", "xy"),
                new VectorData("uvec3", "uvec{0}",4, "int", "rgb", "xyz"),
                new VectorData("uvec4", "uvec{0}",4, "int", "rgba", "xyzw"),
                // STPQ
                new VectorData("uvec2", "uvec{0}",4, "int", "st", "xy"),
                new VectorData("uvec3", "uvec{0}",4, "int", "stp", "xyz"),
                new VectorData("uvec4", "uvec{0}",4, "int", "stpq", "xyzw"),

                // Bool ==========================================
                // XYZW
                new VectorData("bvec2", "bvec{0}",4, "bool",  "xy", "xy", true),
                new VectorData("bvec3", "bvec{0}",4, "bool",  "xyz", "xyz", true),
                new VectorData("bvec4", "bvec{0}",4, "bool",  "xyzw", "xyzw"),
                // RGBA
                new VectorData("bvec2", "bvec{0}",4, "bool", "rg", "xy"),
                new VectorData("bvec3", "bvec{0}",4, "bool", "rgb", "xyz"),
                new VectorData("bvec4", "bvec{0}",4, "bool", "rgba", "xyzw"),
                // STPQ
                new VectorData("bvec2", "bvec{0}",4, "bool", "st", "xy"),
                new VectorData("bvec3", "bvec{0}",4, "bool", "stp", "xyz"),
                new VectorData("bvec4", "bvec{0}",4, "bool", "stpq", "xyzw"),
            };
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(File.ReadAllText("../../../HEADER"));
            sb.AppendLine();

            sb.AppendLine("using UnityEngine;");
            sb.AppendLine();


            sb.AppendLine("namespace UnitySwizzle");
            sb.AppendLine("{");
            foreach (var vectorData in vectors)
            {
                var sbImplem = new StringBuilder();
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
                        string swizzleIndices = "";
                        for (int j = 0; j < permuts.Length; ++j)
                        {
                            var swizzleChar = vectorData.SwizzleChars[permuts[j]];
                            swizzleStr += swizzleChar;
                            swizzleIndices += "" + permuts[j];
                            var accessSwizzle = "_" + vectorData.ExistingAccessor[permuts[j]];
                            swizzleAccess += $"this->{accessSwizzle}";
                            
                            try
                            {

                                swizzleSet += $"this->{accessSwizzle} = ";
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
                                swizzleIndices += ", ";
                            }
                        }
                        string returnName = string.Format(vectorData.ReturnTypeName, i);

                        string ctr = $"{returnName}({swizzleAccess})";
                        if (i == 1)
                        {
                            ctr = $"{swizzleAccess}";
                            returnName = vectorData.DefaultTypeName;
                        }

                        sbImplem.AppendLine($"\t\tTRICIBLE_FORCEINLINE {returnName} {vectorData.Name}::{swizzleStr}() const {{ return {ctr}; }} // readonly");
                        if (NoDuplicatedChar(swizzleStr))
                            sbImplem.AppendLine($"\t\tTRICIBLE_FORCEINLINE swizzle{permuts.Length}<{vectorData.Name}, {vectorData.DefaultTypeName}, {swizzleIndices}> {vectorData.Name}::{swizzleStr}() {{ return {{*this}}; }} // Assignable");

                        sb.AppendLine($"\t\tTRICIBLE_FORCEINLINE {returnName} {swizzleStr}() const; // readonly");
                        if (NoDuplicatedChar(swizzleStr))
                            sb.AppendLine($"\t\tTRICIBLE_FORCEINLINE swizzle{permuts.Length}<{vectorData.Name}, {vectorData.DefaultTypeName}, {swizzleIndices}> {swizzleStr}(); // Assignable");

                        checkNext = NextPermutation(permuts, vectorData.SwizzleChars.Length - 1);
                    }
                    if (i < 4)
                        sb.AppendLine();
                }
                sb.AppendLine("\t}");
                if (vectorData != vectors.Last())
                    sb.AppendLine();
                sb.AppendLine($"\tpublic static class {vectorData.Name + vectorData.SwizzleChars + SUFFIX}CPP Implementation");
                sb.AppendLine("\t{");
                sb.Append(sbImplem);
                sb.AppendLine("\t}");
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
