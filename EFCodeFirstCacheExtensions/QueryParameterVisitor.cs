using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace EFCodeFirstCacheExtensions
{
	/// <summary>
	/// <see cref="ExpressionVisitor"/> subclass which encapsulates logic to 
	/// traverse an expression tree and resolve all the query parameter values
	/// </summary>
	internal class QueryParameterVisitor : ExpressionVisitor
	{
		public QueryParameterVisitor(StringBuilder sb)
		{
			QueryParamBuilder = sb;
			Visited = new Dictionary<int, bool>();
		}

		protected StringBuilder QueryParamBuilder { get; set; }
		protected Dictionary<int, bool> Visited { get; set; }

		public StringBuilder GetQueryParameters(Expression expression)
		{
			Visit(expression);
			return QueryParamBuilder;
		}

		private static object GetMemberValue(MemberExpression memberExpression, Dictionary<int, bool> visited)
		{
			object value;
			if (!TryGetMemberValue(memberExpression, out value, visited))
			{
				UnaryExpression objectMember = Expression.Convert(memberExpression, typeof(object));
				Expression<Func<object>> getterLambda = Expression.Lambda<Func<object>>(objectMember);
				Func<object> getter = null;
				try
				{
					getter = getterLambda.Compile();
				}
				catch (InvalidOperationException)
				{
				}
				if (getter != null) value = getter();
			}
			return value;
		}

		private static bool TryGetMemberValue(Expression expression, out object value, Dictionary<int, bool> visited)
		{
			if (expression == null)
			{
				// used for static fields, etc
				value = null;
				return true;
			}
			// Mark this node as visited (processed)
			int expressionHash = expression.GetHashCode();
			if (!visited.ContainsKey(expressionHash))
			{
				visited.Add(expressionHash, true);
			}
			// Get Member Value, recurse if necessary
			switch (expression.NodeType)
			{
				case ExpressionType.Constant:
					value = ((ConstantExpression)expression).Value;
					return true;
				case ExpressionType.MemberAccess:
					var me = (MemberExpression)expression;
					object target;
					if (TryGetMemberValue(me.Expression, out target, visited))
					{
						// instance target
						switch (me.Member.MemberType)
						{
							case MemberTypes.Field:
								value = ((FieldInfo)me.Member).GetValue(target);
								return true;
							case MemberTypes.Property:
								value = ((PropertyInfo)me.Member).GetValue(target, null);
								return true;
						}
					}
					break;
			}
			// Could not retrieve value
			value = null;
			return false;
		}

		protected override Expression VisitMember(MemberExpression node)
		{
			// Only process nodes that haven't been processed before, this could happen because our traversal
			// is depth-first and will "visit" the nodes in the subtree before this method (VisitMember) does
			if (!Visited.ContainsKey(node.GetHashCode()))
			{
				object value = GetMemberValue(node, Visited);
				if (value != null)
				{
					QueryParamBuilder.Append("\n\r");
					QueryParamBuilder.Append(value.ToString());
				}
			}

			return base.VisitMember(node);
		}
	}
}
