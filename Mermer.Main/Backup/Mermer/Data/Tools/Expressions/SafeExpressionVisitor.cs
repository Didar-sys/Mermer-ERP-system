// Decompiled with JetBrains decompiler
// Type: Mermer.Data.Tools.Expressions.SafeExpressionVisitor
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E04E48D-05E5-40EA-900A-CFBEE8B9F238
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using System;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

#nullable disable
namespace Mermer.Data.Tools.Expressions;

public class SafeExpressionVisitor : LinqKit.ExpressionVisitor
{
  protected override Expression VisitMemberAccess(MemberExpression m)
  {
    Expression expression = this.Visit(m.Expression);
    switch (expression)
    {
      case null:
      case ConstantExpression _:
        object obj1 = ((ConstantExpression) expression)?.Value;
        object obj2 = (object) null;
        Type type = (Type) null;
        FieldInfo member1 = m.Member as FieldInfo;
        if ((object) member1 != null)
        {
          obj2 = member1.GetValue(obj1);
          type = member1.FieldType;
        }
        else
        {
          PropertyInfo member2 = m.Member as PropertyInfo;
          if ((object) member2 != null)
          {
            obj2 = member2.GetIndexParameters().Length == 0 ? member2.GetValue(obj1, (object[]) null) : throw new ArgumentException("cannot eliminate closure references to indexed properties");
            type = member2.PropertyType;
          }
        }
        return (Expression) Expression.Constant(obj2, type);
      default:
        return (Expression) Expression.MakeMemberAccess(expression, m.Member);
    }
  }

  protected override Expression VisitMethodCall(MethodCallExpression m)
  {
    if (!(m.Method.Name == "Contains") || m.Arguments.Count <= 0)
      return base.VisitMethodCall(m);
    IEnumerable enumerable = Expression.Lambda<Func<IEnumerable>>(m.Arguments[0]).Compile()();
    MemberExpression left1 = (MemberExpression) m.Arguments[1];
    Expression left2 = (Expression) null;
    foreach (object obj in enumerable)
    {
      ConstantExpression right1 = Expression.Constant(obj);
      BinaryExpression right2 = Expression.Equal((Expression) left1, (Expression) right1);
      left2 = left2 != null ? (Expression) Expression.OrElse(left2, (Expression) right2) : (Expression) right2;
    }
    return left2;
  }
}
