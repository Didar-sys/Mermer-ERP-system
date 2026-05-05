// Decompiled with JetBrains decompiler
// Type: Mermer.Data.Extenders.MvxPropertyNameExtensionMethods
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E04E48D-05E5-40EA-900A-CFBEE8B9F238
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using System;
using System.Linq.Expressions;
using System.Reflection;

#nullable disable
namespace Mermer.Data.Extenders;

public static class MvxPropertyNameExtensionMethods
{
  private const string WrongExpressionMessage = "Wrong expression\nshould be called with expression like\n() => PropertyName";
  private const string WrongUnaryExpressionMessage = "Wrong unary expression\nshould be called with expression like\n() => PropertyName";

  public static string GetPropertyNameFromExpression<T>(
    this object target,
    Expression<Func<T>> expression)
  {
    PropertyInfo member = ((expression != null ? MvxPropertyNameExtensionMethods.FindMemberExpression<T>(expression) : throw new ArgumentNullException(nameof (expression))) ?? throw new ArgumentException("Wrong expression\nshould be called with expression like\n() => PropertyName", nameof (expression))).Member as PropertyInfo;
    if ((object) member == null)
      throw new ArgumentException("Wrong expression\nshould be called with expression like\n() => PropertyName", nameof (expression));
    if ((object) member.DeclaringType == null)
      throw new ArgumentException("Wrong expression\nshould be called with expression like\n() => PropertyName", nameof (expression));
    if (target != null && !TypeExtensions.IsInstanceOfType(member.DeclaringType, target))
      throw new ArgumentException("Wrong expression\nshould be called with expression like\n() => PropertyName", nameof (expression));
    return !PropertyInfoExtensions.GetGetMethod(member, true).IsStatic ? member.Name : throw new ArgumentException("Wrong expression\nshould be called with expression like\n() => PropertyName", nameof (expression));
  }

  private static MemberExpression FindMemberExpression<T>(Expression<Func<T>> expression)
  {
    if (!(expression.Body is UnaryExpression body))
      return expression.Body as MemberExpression;
    if (body.Operand is MemberExpression operand)
      return operand;
    throw new ArgumentException("Wrong unary expression\nshould be called with expression like\n() => PropertyName", nameof (expression));
  }
}
