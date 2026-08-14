using System.Linq.Expressions;
using System.Reflection;

namespace EasyMapper.Internal;

internal static class MemberExpressionReader
{
    public static PropertyInfo ReadProperty(LambdaExpression expression, string parameterName)
    {
        Expression body = expression.Body;
        if (body is UnaryExpression { NodeType: ExpressionType.Convert } conversion)
        {
            body = conversion.Operand;
        }

        if (body is not MemberExpression { Member: PropertyInfo property } member ||
            member.Expression != expression.Parameters[0])
        {
            throw new ArgumentException(
                "The expression must select a direct property, for example 'value => value.Name'.",
                parameterName);
        }

        return property;
    }
}
