using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace ProjectGenesis
{
    public static class Preloader
    {
        public static IEnumerable<string> TargetDLLs { get; } = new[] { "Assembly-CSharp.dll" };

        public static void Patch(AssemblyDefinition assembly)
        {
            ModuleDefinition module = assembly.MainModule;
            TypeDefinition veinType = module.Types.FirstOrDefault(t => t.FullName == "EVeinType");
            if (veinType == null) return;

            FieldDefinition aluminum = veinType.Fields.FirstOrDefault(i => i.HasDefault && (byte)i.Constant == 15);
            if (aluminum == null)
            {
                aluminum = new FieldDefinition("Aluminum",
                                               FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.Public | FieldAttributes.HasDefault,
                                               veinType) { Constant = 15 };

                veinType.Fields.Add(aluminum);
            }
            else
            {
                veinType.Fields.Remove(aluminum);
                aluminum.Name = "Aluminum";
                veinType.Fields.Add(aluminum);
            }

            var radioactive = new FieldDefinition("Radioactive",
                                                  FieldAttributes.Static |
                                                  FieldAttributes.Literal |
                                                  FieldAttributes.Public |
                                                  FieldAttributes.HasDefault, veinType) { Constant = 16 };

            var tungsten = new FieldDefinition("Tungsten",
                                               FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.Public | FieldAttributes.HasDefault,
                                               veinType) { Constant = 17 };

            veinType.Fields.Add(radioactive);
            veinType.Fields.Add(tungsten);
        }
    }
}
