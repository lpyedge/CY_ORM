using System;
using System.Linq.Expressions;

namespace CY_ORM.Linq
{
    class BinaryExpressionParser : ExpressionParser<BinaryExpression>
    {
        public override void InsertColumn(BinaryExpression expr, ParserArgs args)
        {
            if (expr.NodeType == ExpressionType.Equal)
            {
                LambdaParser.InsertColumn(expr.Left, args);
                args.Builder.Append(',');
            }
        }

        public override void InsertValue(BinaryExpression expr, ParserArgs args)
        {
            if (expr.NodeType == ExpressionType.Equal)
            {
                LambdaParser.InsertValue(expr.Right, args);
                args.Builder.Append(',');
            }
            else
            {
                args.AddParameter(LambdaParser.CompileInvoke(expr), "i");
            }
        }

        public override void UpdateSet(BinaryExpression expr, ParserArgs args)
        {
            if (expr.NodeType == ExpressionType.Equal)
            {
                LambdaParser.UpdateSet(expr.Left, args);
                args.Builder.Append('=');
                LambdaParser.UpdateSet(expr.Right, args);
                args.Builder.Append(',');
            }
            else
            {
                if (ExistsBracket(expr.Left))
                {
                    args.Builder.Append(' ');
                    args.Builder.Append('(');
                    LambdaParser.UpdateSet(expr.Left, args);
                    args.Builder.Append(')');
                }
                else
                {
                    LambdaParser.UpdateSet(expr.Left, args);
                }
                var index = args.Builder.Length;
                if (ExistsBracket(expr.Right))
                {
                    args.Builder.Append(' ');
                    args.Builder.Append('(');
                    LambdaParser.UpdateSet(expr.Right, args);
                    args.Builder.Append(')');
                }
                else
                {
                    LambdaParser.UpdateSet(expr.Right, args);
                }
                var length = args.Builder.Length;
                if (length - index == 5 &&
                    args.Builder[length - 5] == ' ' &&
                    args.Builder[length - 4] == 'N' &&
                    args.Builder[length - 3] == 'U' &&
                    args.Builder[length - 2] == 'L' &&
                    args.Builder[length - 1] == 'L')
                {
                    Sign(expr.NodeType, index, args, true);
                }
                else
                {
                    Sign(expr.NodeType, index, args);
                }

                //if (LambdaParser.GetCodeType(expr.Left) == ExpressionTypeCode.MemberExpression | LambdaParser.GetCodeType(expr.Right) == ExpressionTypeCode.MemberExpression)
                //{
                //    LambdaParser.UpdateSet(expr.Left, args);
                //    switch (expr.NodeType)
                //    {
                //        case ExpressionType.Add:
                //            {
                //                args.Builder.Append('+');
                //            }
                //            break;
                //        case ExpressionType.Subtract:
                //            {
                //                args.Builder.Append('-');
                //            }
                //            break;
                //        case ExpressionType.Multiply:
                //            {
                //                args.Builder.Append('*');
                //            }
                //            break;
                //        case ExpressionType.Divide:
                //            {
                //                args.Builder.Append('/');
                //            }
                //            break;
                //        default:
                //            break;
                //    }
                //    LambdaParser.UpdateSet(expr.Right, args);
                //}
                //else
                //{
                //    args.AddParameter(LambdaParser.CompileInvoke(expr), "u");
                //}

            }
        }

        public override void Where(BinaryExpression expr, ParserArgs args)
        {
            //ExpressionType.AndAlso  &&
            //ExpressionType.OrElse || , |
            if (expr.NodeType == ExpressionType.AndAlso | expr.NodeType == ExpressionType.OrElse
                 | expr.NodeType == ExpressionType.And | expr.NodeType == ExpressionType.Or)
            {
                if (ExistsBracket(expr.Left))
                {
                    args.Builder.Append(' ');
                    args.Builder.Append('(');
                    LambdaParser.Where(expr.Left, args);
                    args.Builder.Append(')');
                }
                else
                {
                    LambdaParser.Where(expr.Left, args);
                }
                var index = args.Builder.Length;
                if (ExistsBracket(expr.Right) 
                    || expr.NodeType == ExpressionType.And || expr.NodeType == ExpressionType.Or)
                {
                    args.Builder.Append(' ');
                    args.Builder.Append('(');
                    LambdaParser.Where(expr.Right, args);
                    args.Builder.Append(')');
                }
                else
                {
                    LambdaParser.Where(expr.Right, args);
                }
                var length = args.Builder.Length;
                if (length - index == 5 &&
                     args.Builder[length - 5] == ' ' &&
                     args.Builder[length - 4] == 'N' &&
                     args.Builder[length - 3] == 'U' &&
                     args.Builder[length - 2] == 'L' &&
                     args.Builder[length - 1] == 'L')
                {
                    Sign(expr.NodeType, index, args, true);
                }
                else
                {
                    Sign(expr.NodeType, index, args);
                }
            }
            else if (LambdaParser.GetCodeType(expr.Left) == ExpressionTypeCode.MemberExpression | LambdaParser.GetCodeType(expr.Left) == ExpressionTypeCode.UnaryExpression)
            {
                if (ExistsBracket(expr.Left))
                {
                    args.Builder.Append(' ');
                    args.Builder.Append('(');
                    LambdaParser.Where(expr.Left, args);
                    args.Builder.Append(')');
                }
                else
                {
                    LambdaParser.Where(expr.Left, args);
                }
                var index = args.Builder.Length;
                if (ExistsBracket(expr.Right))
                {
                    args.Builder.Append(' ');
                    args.Builder.Append('(');
                    LambdaParser.Where(expr.Right, args);
                    args.Builder.Append(')');
                }
                else
                {
                    LambdaParser.Where(expr.Right, args);
                }
                var length = args.Builder.Length;
                if (length - index == 5 &&
                     args.Builder[length - 5] == ' ' &&
                     args.Builder[length - 4] == 'N' &&
                     args.Builder[length - 3] == 'U' &&
                     args.Builder[length - 2] == 'L' &&
                     args.Builder[length - 1] == 'L')
                {
                    Sign(expr.NodeType, index, args, true);
                }
                else
                {
                    Sign(expr.NodeType, index, args);
                }
            }
            else
            {
                LambdaParser.Where(expr, args);
                //args.AddParameter(LambdaParser.CompileInvoke(expr), "u");
            }

            //if (LambdaParser.GetCodeType(expr.Left) == ExpressionTypeCode.MemberExpression |
            //    LambdaParser.GetCodeType(expr.Right) == ExpressionTypeCode.MemberExpression)
            //{
            //    if (ExistsBracket(expr.Left))
            //    {
            //        args.Builder.Append(' ');
            //        args.Builder.Append('(');
            //        LambdaParser.Where(expr.Left, args);
            //        args.Builder.Append(')');
            //    }
            //    else
            //    {
            //        LambdaParser.Where(expr.Left, args);
            //    }
            //    var index = args.Builder.Length;
            //    if (ExistsBracket(expr.Right))
            //    {
            //        args.Builder.Append(' ');
            //        args.Builder.Append('(');
            //        LambdaParser.Where(expr.Right, args);
            //        args.Builder.Append(')');
            //    }
            //    else
            //    {
            //        LambdaParser.Where(expr.Right, args);
            //    }
            //    var length = args.Builder.Length;
            //    if (length - index == 5 &&
            //         args.Builder[length - 5] == ' ' &&
            //         args.Builder[length - 4] == 'N' &&
            //         args.Builder[length - 3] == 'U' &&
            //         args.Builder[length - 2] == 'L' &&
            //         args.Builder[length - 1] == 'L')
            //    {
            //        Sign(expr.NodeType, index, args, true);
            //    }
            //    else
            //    {
            //        Sign(expr.NodeType, index, args);
            //    }
            //}
            //else
            //{
            //    args.AddParameter(LambdaParser.CompileInvoke(expr), "u");
            //}
        }

        /// <summary> 判断是否需要添加括号
        /// </summary>
        private static bool ExistsBracket(Expression expr)
        {
            var s = expr.ToString();
            return s != null && s.Length > 5 && s[0] == '(' && s[1] == '(';
        }

        private static void Sign(ExpressionType type, int index, ParserArgs args, bool useis = false)
        {
            switch (type)
            {
                case ExpressionType.AndAlso:
                    args.Builder.Insert(index, " AND ");
                    break;
                case ExpressionType.And:
                    args.Builder.Insert(index, " AND ");
                    break;
                case ExpressionType.Equal:
                    if (useis)
                    {
                        args.Builder.Insert(index, " IS ");
                    }
                    else
                    {
                        args.Builder.Insert(index, " = ");
                    }
                    break;
                case ExpressionType.GreaterThan:
                    args.Builder.Insert(index, " > ");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    args.Builder.Insert(index, " >= ");
                    break;
                case ExpressionType.NotEqual:
                    if (useis)
                    {
                        args.Builder.Insert(index, " IS NOT ");
                    }
                    else
                    {
                        args.Builder.Insert(index, " <> ");
                    }
                    break;
                case ExpressionType.Or:
                    args.Builder.Insert(index, " OR ");
                    break;
                case ExpressionType.OrElse:
                    args.Builder.Insert(index, " OR ");
                    break;
                case ExpressionType.LessThan:
                    args.Builder.Insert(index, " < ");
                    break;
                case ExpressionType.LessThanOrEqual:
                    args.Builder.Insert(index, " <= ");
                    break;


                case ExpressionType.Add:
                    args.Builder.Insert(index, " + ");
                    break;

                case ExpressionType.Subtract:
                    args.Builder.Insert(index, " - ");
                    break;

                case ExpressionType.Multiply:
                    args.Builder.Insert(index, " * ");
                    break;

                case ExpressionType.Divide:
                    args.Builder.Insert(index, " / ");
                    break;

                default:
                    throw new NotImplementedException("无法解释节点类型" + type);
            }
        }


        public override void Select(BinaryExpression expr, ParserArgs args)
        {
            throw new Exception("未实现方法，内部错误！" + this);
        }

        public override void GroupBy(BinaryExpression expr, ParserArgs args)
        {
            throw new Exception("未实现方法，内部错误！" + this);
        }

        public override void Having(BinaryExpression expr, ParserArgs args)
        {
            throw new Exception("未实现方法，内部错误！" + this);
        }

        public override void OrderBy(BinaryExpression expr, ParserArgs args)
        {
            throw new Exception("未实现方法，内部错误！" + this);
        }

        public override void Object(BinaryExpression expr, ParserArgs args)
        {
            throw new Exception("未实现方法，内部错误！" + this);
        }
    }
}
