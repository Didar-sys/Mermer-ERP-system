using System;

namespace Mermer.Data.Patcher
{
    // Этот атрибут йство при синхронизации/миграции
    [AttributeUsage(AttributeTargets.Property)]
    public class IgnorePatchAttribute : Attribute
    {
    }
}