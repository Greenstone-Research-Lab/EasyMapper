using System.Linq.Expressions;

namespace EasyMapper.Internal;

internal sealed class ParameterReplaceVisitor : ExpressionVisitor
{
    private readonly ParameterExpression _parameter;
    private readonly Expression _replacement;

    public ParameterReplaceVisitor(ParameterExpression parameter, Expression replacement)
    {
        _parameter = parameter;
        _replacement = replacement;
    }

    protected override Expression VisitParameter(ParameterExpression node) =>
        node == _parameter ? _replacement : base.VisitParameter(node);
}
