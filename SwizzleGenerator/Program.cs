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
        public string SwizzleChars;
        public VectorData(string name, string swizzle)
        {
            Name = name;
            SwizzleChars = swizzle;
        }
    }

    class Program
    {
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
                new VectorData("Vector2", "xy"),
                new VectorData("Vector3", "xyz"),
                new VectorData("Vector4", "xyzw"),
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
                sb.AppendLine($"\tpublic static class {vectorData.Name + SUFFIX}");
                sb.AppendLine("\t{");
                for (int i = 2; i < 5; ++i)
                {
                    sb.AppendLine($"\t\t// {vectorData.Name} with {i} components.");

                    int[] permuts = new int[i];
                    bool checkNext = true;
                    while (checkNext)
                    {

                        string swizzleStr = "";
                        string swizzleAccess = "";
                        for (int j = 0; j < permuts.Length; ++j)
                        {
                            var swizzleChar = vectorData.SwizzleChars[permuts[j]];
                            swizzleAccess += $"v.{swizzleChar}";
                            swizzleStr += swizzleChar;
                            if (j < (permuts.Length - 1))
                                swizzleAccess += ", ";
                        }
                        sb.AppendLine($"\t\tpublic static Vector{i} {swizzleStr}(this {vectorData.Name} v) {{ return new Vector{i}({swizzleAccess}); }}");
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
