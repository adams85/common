using System;
using System.Linq.Expressions;

namespace Karambolo.Common
{
    class ParameterReplacerVisitor : ExpressionVisitor
    {
        readonly ParameterExpression _param;
        readonly Expression _expression;

        public ParameterReplacerVisitor(ParameterExpression param)
        {
            _expression = _param = param;
        }

        public ParameterReplacerVisitor(LambdaExpression lambda)
        {
            _expression = lambda.Body;
            _param = lambda.Parameters[0];
        }

        protected override Expression VisitLambda<TLambda>(Expression<TLambda> node)
        {
            return Expression.Lambda(Visit(node.Body), _param);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return _expression;
        }
    }

    public class PredicateBuilder<T>
    {
        public static PredicateBuilder<T> True()
        {
            return new PredicateBuilder<T>(true);
        }

        public static PredicateBuilder<T> False()
        {
            return new PredicateBuilder<T>(false);
        }

        readonly ParameterExpression _param;
        readonly ParameterReplacerVisitor _paramReplacer;
        Expression _body;

        PredicateBuilder(bool value)
        {
            _param = Expression.Parameter(typeof(T));
            _paramReplacer = new ParameterReplacerVisitor(_param);
            _body = Expression.Constant(value);
        }

        public ParameterExpression Param => _param;

        public PredicateBuilder<T> And(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            predicate = _paramReplacer.VisitAndConvert(predicate, null);

            return And(predicate.Body);
        }

        public PredicateBuilder<T> And(Expression predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            _body = Expression.AndAlso(_body, predicate);
            return this;
        }

        public PredicateBuilder<T> Or(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            predicate = _paramReplacer.VisitAndConvert(predicate, null);

            return Or(predicate.Body);
        }

        public PredicateBuilder<T> Or(Expression predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            _body = Expression.OrElse(_body, predicate);
            return this;
        }

        public Expression<Func<T, bool>> Build()
        {
            return Expression.Lambda<Func<T, bool>>(_body, _param);
        }
    }

}
