using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Mermer.Data.Patcher;

public class Patcher : IPatcher
{
    public Patch CreatePatch<T>(T source, T target, string id = null) where T : class
    {
        if (string.IsNullOrEmpty(id))
        {
            PropertyInfo property = typeof(T).GetProperty("Id");
            if (property == null)
                throw new Exception("Source object must have 'Id' property, or value of 'Id' must be passed to the function!");
            if (source == null && target == null)
                throw new Exception("As source and target objects are null, value of 'Id' must be passed to the function!");
            id = property.GetValue(source ?? target)?.ToString();
            if (string.IsNullOrEmpty(id))
                throw new Exception("As source and target objects don't have 'Id' value, value of 'Id' must be passed to the function!");
        }
        Patch patch = new Patch() { Id = id };
        if (source == null)
        {
            patch.Action = PatchAction.Delete;
            return patch;
        }
        patch.Action = target == null ? PatchAction.Create : PatchAction.Update;
        foreach (PropertyInfo property in typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (property.Name != "Id" && property.CanRead && property.CanWrite && !Attribute.IsDefined(property, typeof(IgnorePatchAttribute)))
            {
                object obj1 = property.GetValue(source);
                object obj2 = target == null ? null : property.GetValue(target);

                if (!Equals(obj1, obj2))
                {
                    if (property.PropertyType.IsGenericType && !property.PropertyType.GenericTypeArguments.Any(t => t.IsPrimitive || t == typeof(Decimal) || t == typeof(string)) && obj1 is IList)
                    {
                        Type[] genericTypeArguments = property.PropertyType.GenericTypeArguments;
                        List<Patch> source1 = (List<Patch>)this.GetType().GetMethod(nameof(CreateSubListPatches)).MakeGenericMethod(genericTypeArguments).Invoke(this, new object[] { obj1, obj2 });
                        if (source1.Any())
                        {
                            patch.SubListPatches ??= new Dictionary<string, List<Patch>>();
                            patch.SubListPatches.Add(property.Name, source1);
                        }
                    }
                    else
                    {
                        patch.PropertyPatches ??= new Dictionary<string, object>();
                        patch.PropertyPatches.Add(property.Name, obj1);
                    }
                }
            }
        }
        return patch.Action == PatchAction.Update && (patch.PropertyPatches == null || !patch.PropertyPatches.Any()) && (patch.SubListPatches == null || !patch.SubListPatches.Any()) ? null : patch;
    }

    public List<Patch> CreateSubListPatches<T>(IEnumerable<T> sourceList, IEnumerable<T> targetList) where T : class
    {
        PropertyInfo property = typeof(T).GetProperty("Id");
        if (property == null)
            throw new Exception("Sub list objects must have 'Id' property!");
        List<Patch> source1 = new List<Patch>();
        List<T> objList = targetList?.ToList() ?? new List<T>();
        foreach (T source2 in sourceList)
        {
            string id = property.GetValue(source2)?.ToString();
            T target = default;
            foreach (T obj1 in objList)
            {
                object obj2 = property.GetValue(obj1);
                if (id.Equals(obj2))
                {
                    target = obj1;
                    break;
                }
            }
            objList.Remove(target);
            source1.Add(this.CreatePatch(source2, target, id));
        }
        foreach (T target in objList)
        {
            string id = property.GetValue(target)?.ToString();
            source1.Add(this.CreatePatch(default(T), target, id));
        }
        return source1.Where(x => x != null).ToList();
    }

    public T ApplyPatch<T>(Patch patch, T target) where T : class
    {
        if (patch == null)
            throw new ArgumentNullException(nameof(patch));
        switch (patch.Action)
        {
            case PatchAction.Create:
                if (target != null)
                    throw new Exception($"As this is '{patch.Action}' patch, target object must be null!");
                PropertyInfo property = typeof(T).GetProperty("Id");
                if (property == null)
                    throw new Exception("Target object should have 'Id' property");
                target = Activator.CreateInstance<T>();
                property.SetValue(target, patch.Id);
                goto default;
            case PatchAction.Update:
                if (target == null)
                    break;
                goto default;
            case PatchAction.Delete:
                if (target != null)
                    goto default;
                break;
            default:
                if (patch.Action == PatchAction.Delete)
                    return default;
                if (patch.PropertyPatches?.Any() == true)
                {
                    foreach (KeyValuePair<string, object> propertyPatch in patch.PropertyPatches)
                    {
                        try
                        {
                            string propertyName = propertyPatch.Key;
                            PropertyInfo element = typeof(T).GetProperties().SingleOrDefault(x => x.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
                            if (element != null && element.CanWrite && !Attribute.IsDefined(element, typeof(IgnorePatchAttribute)))
                            {
                                try
                                {
                                    element.SetValue(target, propertyPatch.Value);
                                    continue;
                                }
                                catch { }
                                try
                                {
                                    element.SetValue(target, JsonConvert.DeserializeObject(JsonConvert.SerializeObject(propertyPatch.Value), element.PropertyType));
                                    continue;
                                }
                                catch { }
                                element.SetValue(target, Convert.ChangeType(propertyPatch.Value, element.PropertyType));
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            throw;
                        }
                    }
                }
                if (patch.SubListPatches?.Any() == true)
                {
                    foreach (KeyValuePair<string, List<Patch>> subListPatch in patch.SubListPatches)
                    {
                        string subListName = subListPatch.Key;
                        PropertyInfo element = typeof(T).GetProperties().SingleOrDefault(x => x.Name.Equals(subListName, StringComparison.OrdinalIgnoreCase));
                        if (element != null && element.CanWrite && !Attribute.IsDefined(element, typeof(IgnorePatchAttribute)))
                        {
                            object obj1 = element.GetValue(target) ?? Activator.CreateInstance(element.PropertyType);
                            List<Type> list = element.PropertyType.GenericTypeArguments.ToList();
                            list.Insert(0, element.PropertyType);
                            object obj2 = this.GetType().GetMethod(nameof(ApplySubListPatches)).MakeGenericMethod(list.ToArray()).Invoke(this, new object[] { subListPatch.Value, obj1 });
                            element.SetValue(target, obj2);
                        }
                    }
                }
                return target;
        }
        throw new ArgumentNullException(nameof(target), $"As this is '{patch.Action}' patch, target object can not be null!");
    }

    public TList ApplySubListPatches<TList, T>(List<Patch> patches, TList targetList)
        where TList : IList<T>
        where T : class
    {
        PropertyInfo property = typeof(T).GetProperty("Id");
        if (property == null)
            throw new Exception("Sub list objects must have 'Id' property!");
        foreach (Patch patch in patches)
        {
            if (patch.Action == PatchAction.Create)
            {
                T obj = this.ApplyPatch<T>(patch, default);
                targetList.Add(obj);
            }
            else
            {
                for (int index = 0; index < targetList.Count; ++index)
                {
                    T target = targetList[index];
                    object obj = property.GetValue(target);
                    if (patch.Id.Equals(obj?.ToString()))
                    {
                        if (patch.Action == PatchAction.Update)
                            targetList[index] = this.ApplyPatch(patch, target);
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
            throw new ArgumentNullException(nameof(patch));
        if (laterPatches == null)
            throw new ArgumentNullException(nameof(laterPatches));
        if (patch.Action == PatchAction.Create)
            throw new Exception("Create patch can not be a left behind patch (should be first)");
        if (patch.Action == PatchAction.Delete)
            return patch;
        laterPatches = laterPatches.Where(x => x.Id == patch.Id).ToList();
        if (!laterPatches.Any())
            return patch;
        if (laterPatches.Any(x => x.Action == PatchAction.Delete))
            return null;

        if (patch.PropertyPatches?.Any() == true)
        {
            foreach (string propertyName in patch.PropertyPatches.Keys.ToList())
            {
                if (laterPatches.Where(x => x.PropertyPatches != null).Any(x => x.PropertyPatches.Any(later => later.Key.Equals(propertyName, StringComparison.OrdinalIgnoreCase))))
                    patch.PropertyPatches.Remove(propertyName);
            }
            if (!patch.PropertyPatches.Any())
                patch.PropertyPatches = null;
        }

        if (patch.SubListPatches?.Any() == true)
        {
            foreach (string subListName in patch.SubListPatches.Keys.ToList())
            {
                IEnumerable<Patch> laterSubListPatches = laterPatches.Where(x => x.SubListPatches != null && x.SubListPatches.ContainsKey(subListName))
                                                                     .SelectMany(x => x.SubListPatches[subListName]);

                List<Patch> list = patch.SubListPatches[subListName].Select(x => this.CreatePatchForLeftPatch(x, laterSubListPatches.Where(later => later.Id == x.Id).ToList()))
                                                                    .Where(x => x != null && (x.PropertyPatches?.Any() == true || x.SubListPatches?.Any() == true))
                                                                    .ToList();
                if (list.Any())
                    patch.SubListPatches[subListName] = list;
                else
                    patch.SubListPatches.Remove(subListName);
            }
            if (!patch.SubListPatches.Any())
                patch.SubListPatches = null;
        }
        return patch;
    }
}