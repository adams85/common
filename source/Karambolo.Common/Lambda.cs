using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Karambolo.Common.Properties;

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

        public static MemberExpression GetMemberExpression(LambdaExpression expression, MemberTypes allowedMemberTypes)
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
                    throw new ArgumentException(Resources.InvalidExpression, nameof(expression));

                memberExpressions.Add(memberExpression.Member);
            }
            while (!isSource(currentExpression = memberExpression.Expression));

            memberExpressions.Reverse();
            return memberExpressions;
        }

        public static IEnumerable<MemberInfo> GetMemberPath<TOut>(this Expression<Func<TOut>> expression, MemberTypes allowedMemberTypes = MemberTypes.Property)
        {
            return GetMemberPath(expression, allowedMemberTypes, expr => expr == null);
        }

        public static IEnumerable<MemberInfo> GetMemberPath<T, TOut>(this Expression<Func<T, TOut>> expression, MemberTypes allowedMemberTypes = MemberTypes.Property)
        {
            return GetMemberPath(expression, allowedMemberTypes, expr => expr is ParameterExpression);
        }

        public static FieldInfo Field<TOut>(this Expression<Func<TOut>> expression)
        {
            if (expression == null)
                throw new NullReferenceException();

            var member = GetMemberExpression(expression, MemberTypes.Field);
            if (member == null)
                throw new ArgumentException(null, nameof(expression));
            return (FieldInfo)member.Member;
        }

        public static FieldInfo Field<T, TOut>(this Expression<Func<T, TOut>> expression)
        {
            if (expression == null)
                throw new NullReferenceException();

            var member = GetMemberExpression(expression, MemberTypes.Field);
            if (member == null)
                throw new ArgumentException(null, nameof(expression));
            return (FieldInfo)member.Member;
        }

        public static PropertyInfo Property<TOut>(this Expression<Func<TOut>> expression)
        {
            if (expression == null)
                throw new NullReferenceException();

            var member = GetMemberExpression(expression, MemberTypes.Property);
            if (member == null)
                throw new ArgumentException(null, nameof(expression));
            return (PropertyInfo)member.Member;
        }

        public static PropertyInfo Property<T, TOut>(this Expression<Func<T, TOut>> expression)
        {
            if (expression == null)
                throw new NullReferenceException();

            var member = GetMemberExpression(expression, MemberTypes.Property);
            if (member == null)
                throw new ArgumentException(null, nameof(expression));
            return (PropertyInfo)member.Member;
        }

        public static string PropertyPath<TOut>(Expression<Func<TOut>> expression)
        {
            return string.Join(".", Lambda.GetMemberPath(expression).Select(m => m.Name));
        }

        public static string PropertyPath<T, TOut>(Expression<Func<T, TOut>> expression)
        {
            return string.Join(".", Lambda.GetMemberPath(expression).Select(m => m.Name));
        }

        public static MethodCallExpression GetCallExpression(LambdaExpression expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            return expression.Body as MethodCallExpression;
        }

        public static MethodInfo Method(this Expression<Action> expression)
        {
            if (expression == null)
                throw new NullReferenceException();

            var member = GetCallExpression(expression);
            if (member == null)
                throw new ArgumentException(null, nameof(expression));
            return member.Method;
        }

        public static MethodInfo Method<T>(this Expression<Action<T>> expression)
        {
            if (expression == null)
                throw new NullReferenceException();

            var member = GetCallExpression(expression);
            if (member == null)
                throw new ArgumentException(null, nameof(expression));
            return member.Method;
        }

        public static MethodInfo Method<T, TArg>(this Expression<Action<T, TArg>> expression)
        {
            if (expression == null)
                throw new NullReferenceException();

            var member = GetCallExpression(expression);
            if (member == null)
                throw new ArgumentException(null, nameof(expression));
            return member.Method;
        }

        public static Expression<Func<TIn, TResult>> Substitute<TIn, T, TResult>(this Expression<Func<T, TResult>> expression, Expression<Func<TIn, T>> target)
        {
            if (expression == null)
                throw new NullReferenceException();
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            var paramReplacer = new ParameterReplacerVisitor(target);
            return (Expression<Func<TIn, TResult>>)paramReplacer.Visit(expression);
        }

        public static Action Chain(this Action action, Action otherAction)
        {
            if (action == null)
                throw new NullReferenceException();
            if (otherAction == null)
                throw new ArgumentNullException(nameof(otherAction));

            return () => { action(); otherAction(); };
        }

        public static Action<TArg> Chain<TArg>(this Action<TArg> action, Action<TArg> otherAction)
        {
            if (action == null)
                throw new NullReferenceException();
            if (otherAction == null)
                throw new ArgumentNullException(nameof(otherAction));

            return arg => { action(arg); otherAction(arg); };
        }

        public static Action<TArg1, TArg2> Chain<TArg1, TArg2>(this Action<TArg1, TArg2> action, Action<TArg1, TArg2> otherAction)
        {
            if (action == null)
                throw new NullReferenceException();
            if (otherAction == null)
                throw new ArgumentNullException(nameof(otherAction));

            return (arg1, arg2) => { action(arg1, arg2); otherAction(arg1, arg2); };
        }

        public static MethodInfo MakeGenericMethod(this Expression<Action> expression, params Type[] typeArgs)
        {
            var method = Method(expression);

            if (!method.IsGenericMethod)
                throw new ArgumentException(null, nameof(expression));

            return method.GetGenericMethodDefinition().MakeGenericMethod(typeArgs);
        }

        public static MethodInfo MakeGenericMethod<T>(this Expression<Action<T>> expression, params Type[] typeArgs)
        {
            var method = Method(expression);

            if (!method.IsGenericMethod)
                throw new ArgumentException(null, nameof(expression));

            return method.GetGenericMethodDefinition().MakeGenericMethod(typeArgs);
        }

        public static MethodInfo MakeGenericMethod<T, TArg>(this Expression<Action<T, TArg>> expression, params Type[] typeArgs)
        {
            var method = Method(expression);

            if (!method.IsGenericMethod)
                throw new ArgumentException(null, nameof(expression));

            return method.GetGenericMethodDefinition().MakeGenericMethod(typeArgs);
        }
    }
}

