// Decompiled with JetBrains decompiler
// Type: Mermer.Data.Patcher.Patcher
// Assembly: Mermer.Data.Patcher, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2AD11298-697F-4B7E-AC43-C662A1FFE782
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Data.Patcher.dll

using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TB.ComponentModel;

#nullable disable
namespace Mermer.Data.Patcher;

public class Patcher : IPatcher
{
  public Patch CreatePatch<T>(T source, T target, string id = null) where T : class
  {
    if (string.IsNullOrEmpty(id))
    {
      PropertyInfo property = typeof (T).GetProperty("Id");
      if (property == (PropertyInfo) null)
        throw new Exception("Source object must have 'Id' property, or value of 'Id' must be passed to the function!");
      if ((object) source == null && (object) target == null)
        throw new Exception("As source and target objects are null, value of 'Id' must be passed to the function!");
      id = property.GetValue((object) (source ?? target)).ToString();
      if (string.IsNullOrEmpty(id))
        throw new Exception("As source and target objects don't have 'Id' value, value of 'Id' must be passed to the function!");
    }
    Patch patch = new Patch() { Id = id };
    if ((object) source == null)
    {
      patch.Action = PatchAction.Delete;
      return patch;
    }
    patch.Action = (object) target == null ? PatchAction.Create : PatchAction.Update;
    foreach (PropertyInfo property in typeof (T).GetProperties(BindingFlags.Instance | BindingFlags.Public))
    {
      if (!(property.Name == "Id") && property.CanRead && property.CanWrite && !Attribute.IsDefined((MemberInfo) property, typeof (IgnorePatchAttribute)))
      {
        object obj1 = property.GetValue((object) source);
        object obj2 = (object) target == null ? (object) null : property.GetValue((object) target);
        if ((obj1 != null ? (obj1.Equals(obj2) ? 1 : 0) : (obj2 == null ? 1 : 0)) == 0)
        {
          if (property.PropertyType.IsGenericType && !((IEnumerable<Type>) property.PropertyType.GenericTypeArguments).Any<Type>((Func<Type, bool>) (t => t.IsPrimitive || t == typeof (Decimal) || t == typeof (string))) && obj1 is IList)
          {
            Type[] genericTypeArguments = property.PropertyType.GenericTypeArguments;
            List<Patch> source1 = (List<Patch>) this.GetType().GetMethod("CreateSubListPatches").MakeGenericMethod(genericTypeArguments).Invoke((object) this, new object[2]
            {
              obj1,
              obj2
            });
            if (source1.Any<Patch>())
            {
              patch.SubListPatches = patch.SubListPatches ?? new Dictionary<string, List<Patch>>();
              patch.SubListPatches.Add(property.Name, source1);
            }
          }
          else
          {
            patch.PropertyPatches = patch.PropertyPatches ?? new Dictionary<string, object>();
            patch.PropertyPatches.Add(property.Name, obj1);
          }
        }
      }
    }
    return patch.Action == PatchAction.Update && (patch.PropertyPatches == null || !patch.PropertyPatches.Any<KeyValuePair<string, object>>()) && (patch.SubListPatches == null || !patch.SubListPatches.Any<KeyValuePair<string, List<Patch>>>()) ? (Patch) null : patch;
  }

  public List<Patch> CreateSubListPatches<T>(IEnumerable<T> sourceList, IEnumerable<T> targetList) where T : class
  {
    PropertyInfo property = typeof (T).GetProperty("Id");
    if (property == (PropertyInfo) null)
      throw new Exception("Sub list objects must have 'Id' property!");
    List<Patch> source1 = new List<Patch>();
    List<T> objList = (targetList != null ? targetList.ToList<T>() : (List<T>) null) ?? new List<T>();
    foreach (T source2 in sourceList)
    {
      string id = property.GetValue((object) source2).ToString();
      T target = default (T);
      foreach (T obj1 in objList)
      {
        object obj2 = property.GetValue((object) obj1);
        if (id.Equals(obj2))
        {
          target = obj1;
          break;
        }
      }
      objList.Remove(target);
      source1.Add(this.CreatePatch<T>(source2, target, id));
    }
    foreach (T target in objList)
    {
      string id = property.GetValue((object) target).ToString();
      source1.Add(this.CreatePatch<T>(default (T), target, id));
    }
    return source1.Where<Patch>((Func<Patch, bool>) (x => x != null)).ToList<Patch>();
  }

  public T ApplyPatch<T>(Patch patch, T target) where T : class
  {
    if (patch == null)
      throw new ArgumentNullException(nameof (patch));
    switch (patch.Action)
    {
      case PatchAction.Create:
        if ((object) target != null)
          throw new Exception($"As this is '{patch.Action}' patch, target object must be null!");
        PropertyInfo property = typeof (T).GetProperty("Id");
        if (property == (PropertyInfo) null)
          throw new Exception("Target object should have 'Id' property");
        target = Activator.CreateInstance<T>();
        property.SetValue((object) target, (object) patch.Id);
        goto default;
      case PatchAction.Update:
        if ((object) target == null)
          break;
        goto default;
      case PatchAction.Delete:
        if ((object) target != null)
          goto default;
        break;
      default:
        if (patch.Action == PatchAction.Delete)
          return default (T);
        Dictionary<string, object> propertyPatches = patch.PropertyPatches;
        if ((propertyPatches != null ? (propertyPatches.Any<KeyValuePair<string, object>>() ? 1 : 0) : 0) != 0)
        {
          foreach (KeyValuePair<string, object> propertyPatch in patch.PropertyPatches)
          {
            try
            {
              string propertyName = propertyPatch.Key;
              PropertyInfo element = ((IEnumerable<PropertyInfo>) typeof (T).GetProperties()).SingleOrDefault<PropertyInfo>((Func<PropertyInfo, bool>) (x => x.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase)));
              if (!(element == (PropertyInfo) null))
              {
                if (element.CanWrite)
                {
                  if (!Attribute.IsDefined((MemberInfo) element, typeof (IgnorePatchAttribute)))
                  {
                    try
                    {
                      element.SetValue((object) target, propertyPatch.Value);
                      continue;
                    }
                    catch
                    {
                    }
                    try
                    {
                      element.SetValue((object) target, JsonConvert.DeserializeObject(JsonConvert.SerializeObject(propertyPatch.Value), element.PropertyType));
                      continue;
                    }
                    catch
                    {
                    }
                    element.SetValue((object) target, propertyPatch.Value.Convert(element.PropertyType));
                  }
                }
              }
            }
            catch (Exception ex)
            {
              Console.WriteLine((object) ex);
              throw;
            }
          }
        }
        Dictionary<string, List<Patch>> subListPatches = patch.SubListPatches;
        if ((subListPatches != null ? (subListPatches.Any<KeyValuePair<string, List<Patch>>>() ? 1 : 0) : 0) != 0)
        {
          foreach (KeyValuePair<string, List<Patch>> subListPatch in patch.SubListPatches)
          {
            string subListName = subListPatch.Key;
            PropertyInfo element = ((IEnumerable<PropertyInfo>) typeof (T).GetProperties()).SingleOrDefault<PropertyInfo>((Func<PropertyInfo, bool>) (x => x.Name.Equals(subListName, StringComparison.OrdinalIgnoreCase)));
            if (!(element == (PropertyInfo) null) && element.CanWrite && !Attribute.IsDefined((MemberInfo) element, typeof (IgnorePatchAttribute)))
            {
              object obj1 = element.GetValue((object) target) ?? Activator.CreateInstance(element.PropertyType);
              List<Type> list = ((IEnumerable<Type>) element.PropertyType.GenericTypeArguments).ToList<Type>();
              list.Insert(0, element.PropertyType);
              object obj2 = this.GetType().GetMethod("ApplySubListPatches").MakeGenericMethod(list.ToArray()).Invoke((object) this, new object[2]
              {
                (object) subListPatch.Value,
                obj1
              });
              element.SetValue((object) target, obj2);
            }
          }
        }
        return target;
    }
    throw new ArgumentNullException(nameof (target), $"As this is '{patch.Action}' patch, target object can not be null!");
  }

  public TList ApplySubListPatches<TList, T>(List<Patch> patches, TList targetList)
    where TList : IList<T>
    where T : class
  {
    PropertyInfo property = typeof (T).GetProperty("Id");
    if (property == (PropertyInfo) null)
      throw new Exception("Sub list objects must have 'Id' property!");
    foreach (Patch patch in patches)
    {
      if (patch.Action == PatchAction.Create)
      {
        T obj = this.ApplyPatch<T>(patch, default (T));
        targetList.Add(obj);
      }
      else
      {
        for (int index = 0; index < targetList.Count; ++index)
        {
          T target = targetList[index];
          object obj = property.GetValue((object) target);
          if (patch.Id.Equals(obj))
          {
            if (patch.Action == PatchAction.Update)
              targetList[index] = this.ApplyPatch<T>(patch, target);
            if (patch.Action == PatchAction.Delete)
            {
              targetList.RemoveAt(index);
              break;
            }
            break;
          }
        }
      }
    }
    return targetList;
  }

  public Patch CreatePatchForLeftPatch(Patch patch, List<Patch> laterPatches)
  {
    if (patch == null)
      throw new ArgumentNullException(nameof (patch));
    if (laterPatches == null)
      throw new ArgumentNullException(nameof (laterPatches));
    if (patch.Action == PatchAction.Create)
      throw new Exception("Create patch can not be a left behind patch (should be first)");
    if (patch.Action == PatchAction.Delete)
      return patch;
    laterPatches = laterPatches.Where<Patch>((Func<Patch, bool>) (x => x.Id == patch.Id)).ToList<Patch>();
    if (!laterPatches.Any<Patch>())
      return patch;
    if (laterPatches.Any<Patch>((Func<Patch, bool>) (x => x.Action == PatchAction.Delete)))
      return (Patch) null;
    Dictionary<string, object> propertyPatches1 = patch.PropertyPatches;
    if ((propertyPatches1 != null ? (propertyPatches1.Any<KeyValuePair<string, object>>() ? 1 : 0) : 0) != 0)
    {
      foreach (string str in patch.PropertyPatches.Keys.ToList<string>())
      {
        string propertyName = str;
        if (laterPatches.Where<Patch>((Func<Patch, bool>) (x => x.PropertyPatches != null)).Any<Patch>((Func<Patch, bool>) (x => x.PropertyPatches.Any<KeyValuePair<string, object>>((Func<KeyValuePair<string, object>, bool>) (later => later.Key.Equals(propertyName, StringComparison.OrdinalIgnoreCase))))))
          patch.PropertyPatches.Remove(propertyName);
      }
      if (!patch.PropertyPatches.Any<KeyValuePair<string, object>>())
        patch.PropertyPatches = (Dictionary<string, object>) null;
    }
    Dictionary<string, List<Patch>> subListPatches1 = patch.SubListPatches;
    if ((subListPatches1 != null ? (subListPatches1.Any<KeyValuePair<string, List<Patch>>>() ? 1 : 0) : 0) != 0)
    {
      foreach (string str in patch.SubListPatches.Keys.ToList<string>())
      {
        string subListName = str;
        IEnumerable<Patch> laterSubListPatches = laterPatches.Where<Patch>((Func<Patch, bool>) (x =>
        {
          Dictionary<string, List<Patch>> subListPatches2 = x.SubListPatches;
          // ISSUE: explicit non-virtual call
          return subListPatches2 != null && __nonvirtual (subListPatches2.ContainsKey(subListName));
        })).SelectMany<Patch, Patch>((Func<Patch, IEnumerable<Patch>>) (x => (IEnumerable<Patch>) x.SubListPatches[subListName]));
        List<Patch> list = patch.SubListPatches[subListName].Select<Patch, Patch>((Func<Patch, Patch>) (x => this.CreatePatchForLeftPatch(x, laterSubListPatches.Where<Patch>((Func<Patch, bool>) (later => later.Id == x.Id)).ToList<Patch>()))).Where<Patch>((Func<Patch, bool>) (x =>
        {
          if (x == null)
            return false;
          Dictionary<string, object> propertyPatches2 = x.PropertyPatches;
          if ((propertyPatches2 != null ? (propertyPatches2.Any<KeyValuePair<string, object>>() ? 1 : 0) : 0) != 0)
            return true;
          Dictionary<string, List<Patch>> subListPatches3 = x.SubListPatches;
          return subListPatches3 != null && subListPatches3.Any<KeyValuePair<string, List<Patch>>>();
        })).ToList<Patch>();
        if (list.Any<Patch>())
          patch.SubListPatches[subListName] = list;
        else
          patch.SubListPatches.Remove(subListName);
      }
      if (!patch.SubListPatches.Any<KeyValuePair<string, List<Patch>>>())
        patch.SubListPatches = (Dictionary<string, List<Patch>>) null;
    }
    return patch;
  }
}
