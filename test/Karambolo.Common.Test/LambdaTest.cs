#if NETCOREAPP1_0
using CommonMemberTypes = Karambolo.Common.MemberTypes;
#else
using CommonMemberTypes = System.Reflection.MemberTypes;
#endif

using System;
using System.Linq;
using Xunit;
using System.Reflection;
using System.Linq.Expressions;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Karambolo.Common
{
    public class LambdaTest
    {
        private class Class
        {
            public static int StaticField = 0;
            public static string StaticProperty => StaticField.ToString();
            public static int StaticMethod(string arg) => arg.Length;
            public static TResult StaticGenericMethod<TArg, TResult>(TArg arg) => default;

            public int Field = 0;
            public string Property => Field.ToString();
            public int Method(string arg) => arg.Length;
            public TResult GenericMethod<TArg, TResult>(TArg arg) => default;
        }

        [Fact]
        public void GetMemberExpressionTest()
        {
            Expression<Func<Class, int>> fieldExpr = c => c.Field;
            MemberExpression memberExpr = Lambda.GetMemberExpression(fieldExpr, CommonMemberTypes.Property);
            Assert.Null(memberExpr);

            memberExpr = Lambda.GetMemberExpression(fieldExpr, CommonMemberTypes.Field);
            Assert.NotNull(memberExpr);
            Assert.Equal(typeof(Class).GetTypeInfo().GetDeclaredField(nameof(Class.Field)), memberExpr.Member);

            Expression<Func<Class, string>> propertyExpr = c => c.Property;
            memberExpr = Lambda.GetMemberExpression(propertyExpr, CommonMemberTypes.Field);
            Assert.Null(memberExpr);

            memberExpr = Lambda.GetMemberExpression(propertyExpr, CommonMemberTypes.Property);
            Assert.NotNull(memberExpr);
            Assert.Equal(typeof(Class).GetTypeInfo().GetDeclaredProperty(nameof(Class.Property)), memberExpr.Member);

            Expression<Func<Class, int>> nonMemberExpr = c => c.Method(default);
            memberExpr = Lambda.GetMemberExpression(nonMemberExpr, CommonMemberTypes.Field | CommonMemberTypes.Property);
            Assert.Null(memberExpr);
        }

        [Fact]
        public void GetMemberPathTest()
        {
            Assert.Equal("Now.Day", string.Join(".", Lambda.GetMemberPath(() => DateTime.Now.Day).Select(m => m.Name)));

            Assert.Throws<ArgumentException>(() => Lambda.GetMemberPath(() => DateTime.MaxValue));

            Assert.Equal("MaxValue", string.Join(".", Lambda.GetMemberPath(() => DateTime.MaxValue, CommonMemberTypes.Field).Select(m => m.Name)));

            Assert.Equal("Date.Day", string.Join(".", Lambda.GetMemberPath((DateTime dt) => dt.Date.Day).Select(m => m.Name)));

            Assert.Throws<ArgumentException>(() => Lambda.GetMemberPath(() => DateTime.MaxValue, CommonMemberTypes.Field | CommonMemberTypes.Event));
        }

        [Fact]
        public void MemberPathTest()
        {
            Assert.Equal("Now.Day", string.Join(".", Lambda.MemberPath(() => DateTime.Now.Day)));

            Assert.Throws<ArgumentException>(() => Lambda.MemberPath(() => DateTime.MaxValue.Hour));

            Assert.Equal("MaxValue.Hour", Lambda.MemberPath(() => DateTime.MaxValue.Hour, CommonMemberTypes.Field | CommonMemberTypes.Property));

            Assert.Equal("Date.Day", string.Join(".", Lambda.MemberPath((DateTime dt) => dt.Date.Day)));

            Assert.Equal("Now", string.Join(".", Lambda.MemberPath<object>(() => DateTime.Now)));
            Assert.Equal("Task.Result.Length", string.Join(".", Lambda.MemberPath((object o) => ((string)((TaskCompletionSource<object>)o).Task.Result).Length)));
        }

        [Fact]
        public void FieldTest()
        {
            FieldInfo field = Lambda.Field(() => Class.StaticField);
            Assert.Equal(typeof(Class).GetTypeInfo().GetDeclaredField(nameof(Class.StaticField)), field);

            field = Lambda.Field((Class c) => c.Field);
            Assert.Equal(typeof(Class).GetTypeInfo().GetDeclaredField(nameof(Class.Field)), field);

            Assert.Throws<ArgumentException>(() => Lambda.Field(() => Class.StaticProperty));
        }

        [Fact]
        public void PropertyTest()
        {
            PropertyInfo property = Lambda.Property(() => Class.StaticProperty);
            Assert.Equal(typeof(Class).GetTypeInfo().GetDeclaredProperty(nameof(Class.StaticProperty)), property);

            property = Lambda.Property((Class c) => c.Property);
            Assert.Equal(typeof(Class).GetTypeInfo().GetDeclaredProperty(nameof(Class.Property)), property);

            Assert.Throws<ArgumentException>(() => Lambda.Property(() => Class.StaticField));
        }

        [Fact]
        public void GetCallExpressionTest()
        {
            Expression<Func<int>> callExpr = () => Class.StaticMethod(default);
            MethodCallExpression methodCallExpr = Lambda.GetCallExpression(callExpr);
            Assert.Equal(typeof(Class).GetTypeInfo().GetDeclaredMethod(nameof(Class.StaticMethod)), methodCallExpr.Method);

            Expression<Func<int>> nonCallExpr = () => Class.StaticField;
            methodCallExpr = Lambda.GetCallExpression(nonCallExpr);
            Assert.Null(methodCallExpr);
        }

        [Fact]
        public void MethodTest()
        {
            MethodInfo method = Lambda.Method(() => Class.StaticMethod(default));
            Assert.Equal(typeof(Class).GetTypeInfo().GetDeclaredMethod(nameof(Class.StaticMethod)), method);

            method = Lambda.Method((Class c) => c.Method(default));
            Assert.Equal(typeof(Class).GetTypeInfo().GetDeclaredMethod(nameof(Class.Method)), method);
        }

        [Fact]
        public void MakeGenericMethodTest()
        {
            Expression<Action> callExpr = () => Class.StaticGenericMethod<object, object>(default);
            MethodCallExpression methodCallExpr = Lambda.GetCallExpression(callExpr);
            MethodInfo method = Lambda.MakeGenericMethod(methodCallExpr, typeof(string), typeof(int));
            Assert.True(method.IsGenericMethod);
            Assert.False(method.IsGenericMethodDefinition);
            Assert.Equal(typeof(Class).GetTypeInfo().GetDeclaredMethod(nameof(Class.StaticGenericMethod)), method.GetGenericMethodDefinition());
            Assert.Equal(new[] { typeof(string), typeof(int) }, method.GetGenericArguments());

            method = Lambda.MakeGenericMethod(() => Class.StaticGenericMethod<object, object>(default), typeof(string), typeof(int));
            Assert.True(method.IsGenericMethod);
            Assert.False(method.IsGenericMethodDefinition);
            Assert.Equal(typeof(Class).GetTypeInfo().GetDeclaredMethod(nameof(Class.StaticGenericMethod)), method.GetGenericMethodDefinition());
            Assert.Equal(new[] { typeof(string), typeof(int) }, method.GetGenericArguments());

            method = Lambda.MakeGenericMethod((Class c) => c.GenericMethod<object, object>(default), typeof(string), typeof(int));
            Assert.True(method.IsGenericMethod);
            Assert.False(method.IsGenericMethodDefinition);
            Assert.Equal(typeof(Class).GetTypeInfo().GetDeclaredMethod(nameof(Class.GenericMethod)), method.GetGenericMethodDefinition());
            Assert.Equal(new[] { typeof(string), typeof(int) }, method.GetGenericArguments());
        }

        [Fact]
        public void ChainTest()
        {
            Expression<Func<int, Class>> expr1 = value => new Class { Field = value };
            Expression<Func<Class, string>> expr2 = c => c.Property;

            Expression<Func<int, string>> chainedExpr = expr1.Chain(expr2);
            Assert.Equal("1", chainedExpr.Compile()(1));

            Func<int, Class> func1 = expr1.Compile();
            Func<Class, string> func2 = expr2.Compile();

            Func<int, string> chainedFunc = func1.Chain(func2);
            Assert.Equal("2", chainedFunc(2));

            var list = new List<int>();
            Lambda.Chain(() => list.Add(1), () => list.Add(2))();
            Assert.Equal(new[] { 1, 2 }, list);

            Lambda.Chain((int i) => list.Add(i + 3), (int i) => list.Add(i + 4))(0);
            Assert.Equal(new[] { 1, 2, 3, 4 }, list);
        }
    }
}
