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

        private static TypeDefinition GetTypeByName(this AssemblyDefinition assembly, string name) => assembly.MainModule.Types.FirstOrDefault(t => t.FullName == name);

        private static FieldDefinition GetFieldByName(this TypeDefinition type, string name) => type.Fields.FirstOrDefault(t => t.Name == name);

        private static void AddEnumField(this TypeDefinition type, string name, object constant, FieldAttributes fieldAttributes) =>
            type.Fields.Add(new FieldDefinition(name, fieldAttributes, type) { Constant = constant, });

        private static void AddTypeField(this AssemblyDefinition assembly, string typeName, string oriFieldName, string newFieldName)
        {
            TypeDefinition type = assembly.GetTypeByName(typeName);
            FieldDefinition oriField = type.GetFieldByName(oriFieldName);
            type.Fields.Add(new FieldDefinition(newFieldName, oriField.Attributes, oriField.FieldType));
        }

        public static void Patch(AssemblyDefinition assembly)
        {
            TypeDefinition veinType = assembly.GetTypeByName("EVeinType");
            FieldDefinition max = veinType.Fields.FirstOrDefault(i => i.HasDefault && (byte)i.Constant == 15);

            if (max != null) veinType.Fields.Remove(max);

            FieldAttributes fieldAttributes = FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.Public | FieldAttributes.HasDefault;

            veinType.AddEnumField("Aluminum", 15, fieldAttributes);
            veinType.AddEnumField("Radioactive", 16, fieldAttributes);
            veinType.AddEnumField("Tungsten", 17, fieldAttributes);
            veinType.AddEnumField("Sulfur", 18, fieldAttributes);

            assembly.AddTypeField("PlanetData", "birthResourcePoint0", "birthResourcePoint2");
            assembly.AddTypeField("GameDesc", "isSandboxMode", "isFastStartMode");
        }
    }
}
