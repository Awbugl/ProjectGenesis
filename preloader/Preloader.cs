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

            bool flag = veinType == null;
            if (flag) return;


            var aluminum = new FieldDefinition("Aluminum",
                                               FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.Public | FieldAttributes.HasDefault,
                                               veinType) { Constant = 16 };

            var tungsten = new FieldDefinition("Tungsten",
                                               FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.Public | FieldAttributes.HasDefault,
                                               veinType) { Constant = 17 };

            var radioactive = new FieldDefinition("Radioactive",
                                                  FieldAttributes.Static |
                                                  FieldAttributes.Literal |
                                                  FieldAttributes.Public |
                                                  FieldAttributes.HasDefault, veinType) { Constant = 18 };

            veinType.Fields.Add(aluminum);
            veinType.Fields.Add(tungsten);
            veinType.Fields.Add(radioactive);
        }
    }
}
