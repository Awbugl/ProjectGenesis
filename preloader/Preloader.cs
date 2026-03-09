using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using FieldAttributes = Mono.Cecil.FieldAttributes;

[assembly:AssemblyVersion(ProjectGenesis.ProjectGenesis.VERSION)]

namespace ProjectGenesis
{
    public static class Preloader
    {
        public static IEnumerable<string> TargetDLLs { get; } = new[] { "Assembly-CSharp.dll", };

        private static TypeDefinition GetTypeByName(this AssemblyDefinition assembly, string name) =>
            assembly.MainModule.Types.FirstOrDefault(t => t.FullName == name);

        private static TypeDefinition GetInnerTypeByName(this AssemblyDefinition assembly, string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            // 先尝试直接匹配（非嵌套类型）
            var directType = assembly.GetTypeByName(name);
            if (directType != null) return directType;

            // 处理嵌套类型：找到最后一个 '.' 的位置，分割为 "OuterTypePath" 和 "InnerTypeName"
            // 例如 "A.B.C.D" -> 外层 "A.B.C", 内层 "D"
            var lastDotIndex = name.LastIndexOf('.');
            if (lastDotIndex < 0) return null;

            var outerPath = name.Substring(0, lastDotIndex);
            var innerName = name.Substring(lastDotIndex + 1);

            // 递归查找外层类型
            var outerType = assembly.GetInnerTypeByName(outerPath);

            // 在外层类型的 NestedTypes 中查找内层
            return outerType?.NestedTypes.FirstOrDefault(t => t.Name == innerName);
        }

        private static FieldDefinition GetFieldByName(this TypeDefinition type, string name) =>
            type.Fields.FirstOrDefault(t => t.Name == name);

        private static void AddEnumField(this TypeDefinition type, string name, object constant, FieldAttributes fieldAttributes) =>
            type.Fields.Add(new FieldDefinition(name, fieldAttributes, type) { Constant = constant, });

        private static void AddTypeField(this AssemblyDefinition assembly, string typeName, string oriFieldName, string newFieldName,
            bool notSerialized = false)
        {
            TypeDefinition type = assembly.GetTypeByName(typeName);
            FieldDefinition oriField = type.GetFieldByName(oriFieldName);
            var fieldDefinition = new FieldDefinition(newFieldName, oriField.Attributes, oriField.FieldType);

            if (notSerialized) fieldDefinition.Attributes |= FieldAttributes.NotSerialized;

            type.Fields.Add(fieldDefinition);
        }

        public static void Patch(AssemblyDefinition assembly)
        {
            TypeDefinition veinType = assembly.GetTypeByName("EVeinType");
            FieldDefinition max = veinType.Fields.FirstOrDefault(i => i.HasDefault && (byte)i.Constant == 15);

            if (max != null) veinType.Fields.Remove(max);

            FieldAttributes fieldAttributes =
                FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.Public | FieldAttributes.HasDefault;

            veinType.AddEnumField("Aluminum", 15, fieldAttributes);
            veinType.AddEnumField("Radioactive", 16, fieldAttributes);
            veinType.AddEnumField("Niobium", 17, fieldAttributes);
            veinType.AddEnumField("Sulfur", 18, fieldAttributes);
            veinType.AddEnumField("Salt", 19, fieldAttributes);
            veinType.AddEnumField("Tholin", 20, fieldAttributes);

            assembly.AddTypeField("PlanetData", "birthResourcePoint0", "birthResourcePoint2");
            assembly.AddTypeField("PlanetData", "birthResourcePoint0", "birthResourcePoint3");
            assembly.AddTypeField("PlanetData", "birthResourcePoint0", "birthResourcePoint4");

            assembly.AddTypeField("GameDesc", "isSandboxMode", "isFastStartMode");

            assembly.AddTypeField("RecipeProto", "TimeSpend", "PowerFactor", true);
            assembly.AddTypeField("RecipeProto", "TimeSpend", "Overflow", true);
        }
    }
}
