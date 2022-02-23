using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using CommonAPI;

namespace ProjectGenesis
{
    public static class DeepCopyHelper
    {
        public static T DeepCopy<T>(this T obj) where T : new()
        {
            if (obj is string || obj.GetType().IsValueType) return obj;

            var retval = new T();
            var fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (field.GetCustomAttribute<NonSerializedAttribute>() == null)
                    if (field.FieldType == typeof(Int32[]))
                        field.SetValue(retval, new int[] { });
                    else if (field.FieldType == typeof(UnityEngine.Sprite))
                        field.SetValue(retval, (UnityEngine.Sprite)field.GetValue(obj));
                    else
                        field.SetValue(retval, DeepCopy(Convert.ChangeType(field.GetValue(obj), field.FieldType)));
            }

            return retval;
        }


        public static T DeepCopyByBinary<T>(this T obj)
        {
            object retval;
            using (var ms = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                bf.Serialize(ms, obj);
                ms.Seek(0, SeekOrigin.Begin);
                retval = bf.Deserialize(ms);
                ms.Close();
            }

            return (T)retval;
        }

        public static ItemProto ItemProtoDeepCopy(this ItemProto obj)
        {
            var retval = obj.DeepCopy();
            retval.isRaw = obj.isRaw;
            retval.handcraft = new RecipeProto();
            retval.maincraft = new RecipeProto();
            obj.handcraft.CopyPropsTo(ref retval.handcraft);
            obj.maincraft.CopyPropsTo(ref retval.maincraft);
            retval.handcraftProductCount = obj.handcraftProductCount;
            retval.maincraftProductCount = obj.maincraftProductCount;
            retval.handcrafts = new List<RecipeProto>(obj.handcrafts.ToArray());
            retval.recipes = new List<RecipeProto>(obj.recipes.ToArray());
            retval.makes = new List<RecipeProto>(obj.makes.ToArray());
            retval.rawMats = new List<IDCNT>(obj.rawMats.ToArray());
            retval.preTech = obj.preTech;
            retval.missingTech = obj.missingTech;
            retval.prefabDesc = PrefabDesc.none;
            obj.prefabDesc.CopyPropsTo(ref retval.prefabDesc);
            return retval;
        }
    }
}
