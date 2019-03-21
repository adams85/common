using System;
using System.Linq.Expressions;
using Karambolo.Common.Internal;

namespace Karambolo.Common
{
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

        private readonly ParameterExpression _param;
        private readonly ParameterReplacerVisitor _paramReplacer;
        private Expression _body;

        private PredicateBuilder(bool value)
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
