using System.Linq.Expressions;

namespace Karambolo.Common.Internal
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

}
