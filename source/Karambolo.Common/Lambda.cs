using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Karambolo.Common.Properties;
using Karambolo.Common.Internal;

namespace Karambolo.Common
{
    public static class Lambda
    {
        static MemberExpression GetMemberExpression(Expression expression, MemberTypes allowedMemberTypes)
        {
            return
                expression is MemberExpression memberExpression && (allowedMemberTypes & memberExpression.Member.MemberType()) != 0 ?
                memberExpression :
                null;
        }

        public static MemberExpression GetMemberExpression(this LambdaExpression expression, MemberTypes allowedMemberTypes)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            return GetMemberExpression(expression.Body, allowedMemberTypes);
        }

        static IEnumerable<MemberInfo> GetMemberPath(LambdaExpression expression, MemberTypes allowedMemberTypes, Func<Expression, bool> isSource)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            if ((allowedMemberTypes & ~(MemberTypes.Field | MemberTypes.Property)) != 0)
                throw new ArgumentException(Resources.FieldOrPropertyAllowedOnly, nameof(allowedMemberTypes));

            var memberExpressions = new List<MemberInfo>();
            var currentExpression = expression.Body;
            MemberExpression memberExpression;
            do
            {
                memberExpression =
                    GetMemberExpression(currentExpression, allowedMemberTypes) ??
                    throw new ArgumentException(Resources.InvalidValue, nameof(expression));

                memberExpressions.Add(memberExpression.Member);
            }
            while (!isSource(currentExpression = memberExpression.Expression));

            memberExpressions.Reverse();
            return memberExpressions;
        }

        public static IEnumerable<MemberInfo> GetMemberPath<TMember>(this Expression<Func<TMember>> expression, MemberTypes allowedMemberTypes = MemberTypes.Property)
        {
            return GetMemberPath(expression, allowedMemberTypes, expr => expr == null);
        }

        public static IEnumerable<MemberInfo> GetMemberPath<TRoot, TMember>(this Expression<Func<TRoot, TMember>> expression, MemberTypes allowedMemberTypes = MemberTypes.Property)
        {
            return GetMemberPath(expression, allowedMemberTypes, expr => expr is ParameterExpression);
        }

        public static string MemberPath<TProperty>(this Expression<Func<TProperty>> expression, MemberTypes allowedMemberTypes = MemberTypes.Property)
        {
            return string.Join(".", GetMemberPath(expression, allowedMemberTypes).Select(m => m.Name));
        }

        public static string MemberPath<TRoot, TProperty>(this Expression<Func<TRoot, TProperty>> expression, MemberTypes allowedMemberTypes = MemberTypes.Property)
        {
            return string.Join(".", GetMemberPath(expression, allowedMemberTypes).Select(m => m.Name));
        }

        public static FieldInfo Field<TField>(this Expression<Func<TField>> expression)
        {
            var member = GetMemberExpression(expression, MemberTypes.Field);
            if (member == null)
                throw new ArgumentException(Resources.InvalidValue, nameof(expression));

            return (FieldInfo)member.Member;
        }

        public static FieldInfo Field<TContainer, TField>(this Expression<Func<TContainer, TField>> expression)
        {
            var member = GetMemberExpression(expression, MemberTypes.Field);
            if (member == null)
                throw new ArgumentException(Resources.InvalidValue, nameof(expression));

            return (FieldInfo)member.Member;
        }

        public static PropertyInfo Property<TProperty>(this Expression<Func<TProperty>> expression)
        {
            var member = GetMemberExpression(expression, MemberTypes.Property);
            if (member == null)
                throw new ArgumentException(Resources.InvalidValue, nameof(expression));

            return (PropertyInfo)member.Member;
        }

        public static PropertyInfo Property<TContainer, TProperty>(this Expression<Func<TContainer, TProperty>> expression)
        {
            var member = GetMemberExpression(expression, MemberTypes.Property);
            if (member == null)
                throw new ArgumentException(Resources.InvalidValue, nameof(expression));

            return (PropertyInfo)member.Member;
        }

        public static MethodCallExpression GetCallExpression(this LambdaExpression expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            return expression.Body as MethodCallExpression;
        }

        public static MethodInfo Method(this Expression<Action> expression)
        {
            var member = GetCallExpression(expression);
            if (member == null)
                throw new ArgumentException(Resources.InvalidValue, nameof(expression));

            return member.Method;
        }

        public static MethodInfo Method<TContainer>(this Expression<Action<TContainer>> expression)
        {
            var member = GetCallExpression(expression);
            if (member == null)
                throw new ArgumentException(Resources.InvalidValue, nameof(expression));

            return member.Method;
        }

        public static MethodInfo MakeGenericMethod(this MethodCallExpression expression, params Type[] typeArgs)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            var method = expression.Method;

            return method.GetGenericMethodDefinition().MakeGenericMethod(typeArgs);
        }

        public static MethodInfo MakeGenericMethod(this Expression<Action> expression, params Type[] typeArgs)
        {
            var method = Method(expression);

            return method.GetGenericMethodDefinition().MakeGenericMethod(typeArgs);
        }

        public static MethodInfo MakeGenericMethod<TContainer>(this Expression<Action<TContainer>> expression, params Type[] typeArgs)
        {
            var method = Method(expression);

            return method.GetGenericMethodDefinition().MakeGenericMethod(typeArgs);
        }

        public static Expression<Func<T, TResult>> Chain<T, TIntermediate, TResult>(this Expression<Func<T, TIntermediate>> expression, Expression<Func<TIntermediate, TResult>> otherExpression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            if (otherExpression == null)
                throw new ArgumentNullException(nameof(otherExpression));

            var paramReplacer = new ParameterReplacerVisitor(expression);
            return (Expression<Func<T, TResult>>)paramReplacer.Visit(otherExpression);
        }

        public static Func<T, TResult> Chain<T, TIntermediate, TResult>(this Func<T, TIntermediate> func, Func<TIntermediate, TResult> otherFunc)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (otherFunc == null)
                throw new ArgumentNullException(nameof(otherFunc));

            return arg => otherFunc(func(arg));
        }

        public static Action Chain(this Action action, Action otherAction)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (otherAction == null)
                throw new ArgumentNullException(nameof(otherAction));

            return () => { action(); otherAction(); };
        }

        public static Action<TArg> Chain<TArg>(this Action<TArg> action, Action<TArg> otherAction)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (otherAction == null)
                throw new ArgumentNullException(nameof(otherAction));

            return arg => { action(arg); otherAction(arg); };
        }
    }
}
