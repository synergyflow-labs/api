using System.Linq.Expressions;
using System.Text;

namespace SynergyFlow.Application.SubcutaneousTests.Common;

public static class ValidationTestExtensions
{
    public static string GetPropertyPath<T>(Expression<Func<T, object?>> expression)
    {
        var body = expression.Body;

        if (body is UnaryExpression unary && unary.NodeType == ExpressionType.Convert)
        {
            body = unary.Operand;
        }

        var path = new StringBuilder();
        while (body is MemberExpression member)
        {
            if (path.Length > 0)
            {
                path.Insert(0, ".");
            }

            path.Insert(0, member.Member.Name);
            body = member.Expression;
        }

        return path.ToString();
    }
}
