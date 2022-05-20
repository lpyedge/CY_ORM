using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace CY_ORM
{
    internal static class Reflection
    {
         /// <summary>
        /// 对象属性 设置和获取
        /// </summary>
        public static class Property
        {
            static readonly ConcurrentDictionary<string, Type> m_cacheTypes = new ConcurrentDictionary<string, Type>();

            static readonly ConcurrentDictionary<string, dynamic> m_cacheDelegates = new ConcurrentDictionary<string, dynamic>();

            private static dynamic SetDelegate<T>(PropertyInfo property)
            {
                var DeclaringExp = Expression.Parameter(typeof(T), "obj");
                var PropertyExp = Expression.Parameter(typeof(object), "val");
                var PropertyConvertExp = Expression.Convert(PropertyExp, property.PropertyType);
                var Exp = Expression.Call(DeclaringExp, property.GetSetMethod(), PropertyConvertExp);
                return Expression.Lambda<Action<T, dynamic>>(Exp, DeclaringExp, PropertyExp).Compile();
            }
            private static Func<T, dynamic> GetDelegate<T>(PropertyInfo property)
            {
                var DeclaringExp = Expression.Parameter(typeof(T), "obj");
                var PropertyExp = Expression.Property(DeclaringExp, property);
                var PropertyConvertExp = Expression.Convert(PropertyExp, typeof(object));
                return Expression.Lambda<Func<T, dynamic>>(PropertyConvertExp, DeclaringExp).Compile();
            }

            public static void SetValue<T>(T t, Expression<Func<T, dynamic>> expression, dynamic value) where T : class
            {
                if (expression != null && (expression.Body is MemberExpression | expression.Body is UnaryExpression))
                {
                    var propertyname = expression.Body is UnaryExpression ? ((expression.Body as UnaryExpression).Operand as MemberExpression).Member.Name : (expression.Body as MemberExpression).Member.Name;
                    SetValue(t, propertyname, value);
                }
                else
                {
                    throw new Exception("lambda表达式有误！");
                }
            }

            public static dynamic GetValue<T>(T t, Expression<Func<T, dynamic>> expression) where T : class
            {
                if (expression != null && (expression.Body is MemberExpression | expression.Body is UnaryExpression))
                {
                    var propertyname = expression.Body is UnaryExpression ? ((expression.Body as UnaryExpression).Operand as MemberExpression).Member.Name : (expression.Body as MemberExpression).Member.Name;
                    return GetValue(t, propertyname);
                }
                else
                {
                    throw new Exception("lambda表达式有误！");
                }
            }

            public static void SetValue<T>(T t, string propertyname, dynamic value) where T : class
            {
                string key = typeof(T).FullName + propertyname;
                if (!m_cacheDelegates.ContainsKey(key + "set"))
                {
                    PropertyInfo property = typeof(T).GetProperty(propertyname);
                    m_cacheDelegates[key + "set"] = SetDelegate<T>(property);
                    m_cacheTypes[key] = property.PropertyType;
                }
                if (value.GetType() == m_cacheTypes[key])
                    m_cacheDelegates[key + "set"](t, value);
                else
                    m_cacheDelegates[key + "set"](t, Convert.ChangeType(value, m_cacheTypes[key]));
            }

            public static dynamic GetValue<T>(T t, string propertyname) where T : class
            {
                string key = typeof(T).FullName + propertyname;
                if (!m_cacheDelegates.ContainsKey(key + "get"))
                {
                    PropertyInfo property = typeof(T).GetProperty(propertyname);
                    m_cacheDelegates[key + "get"] = GetDelegate<T>(property);
                }
                return m_cacheDelegates[key + "get"](t);
            }

            //static readonly ConcurrentDictionary<string, Method.MethodInvokeHandler> m_cacheFastInvokeHandlers = new ConcurrentDictionary<string, Method.MethodInvokeHandler>();

            //public static void SetValue_Method<T>(T t, string propertyname, dynamic value)
            //{
            //    string key = typeof(T).FullName + propertyname + "set";
            //    if (!m_cacheFastInvokeHandlers.ContainsKey(key))
            //    {
            //        PropertyInfo property = typeof(T).GetProperty(propertyname);
            //        MethodInfo methodinfo = property.GetSetMethod();
            //        m_cacheFastInvokeHandlers[key] = Method.GetMethodInvoker(methodinfo);
            //    }
            //    m_cacheFastInvokeHandlers[key].Invoke(t, value);
            //}

            //public static dynamic GetValue_Method<T>(T t, string propertyname)
            //{
            //    string key = typeof(T).FullName + propertyname + "get";
            //    if (!m_cacheFastInvokeHandlers.ContainsKey(key))
            //    {
            //        PropertyInfo property = typeof(T).GetProperty(propertyname);
            //        MethodInfo methodinfo = property.GetGetMethod();
            //        m_cacheFastInvokeHandlers[key] = Method.GetMethodInvoker(methodinfo);
            //    }
            //    return m_cacheFastInvokeHandlers[key].Invoke(t);
            //}
        }


        /// <summary>
        /// 对象方法 快速调用和创建方法委托
        /// </summary>
        public static class Method
        {
            static readonly ConcurrentDictionary<string, MethodInvokeHandler> m_cacheHandlers = new ConcurrentDictionary<string, MethodInvokeHandler>();

            public static dynamic Invoker(dynamic target, MethodInfo methodinfo, params dynamic[] paramters)
            {
                if (methodinfo != null && methodinfo.DeclaringType != null)
                {
                    if (methodinfo.IsStatic)
                    {
                        string key = methodinfo.DeclaringType.FullName + methodinfo.Name;
                        MethodInvokeHandler invokeHandler;
                        if (m_cacheHandlers.ContainsKey(key))
                        {
                            invokeHandler = m_cacheHandlers[key];
                        }
                        else
                        {
                            invokeHandler = GetMethodInvoker(methodinfo);
                            m_cacheHandlers[key] = invokeHandler;
                        }
                        return invokeHandler.Invoke(null, paramters);
                    }
                    else if (target != null)
                    {
                        string key = methodinfo.DeclaringType.FullName + methodinfo.Name;
                        MethodInvokeHandler invokeHandler;
                        if (m_cacheHandlers.ContainsKey(key))
                        {
                            invokeHandler = m_cacheHandlers[key];
                        }
                        else
                        {
                            invokeHandler = GetMethodInvoker(methodinfo);
                            m_cacheHandlers[key] = invokeHandler;
                        }
                        return invokeHandler.Invoke(target, paramters);
                    }
                }
                return null;
            }

            public static dynamic Invoker(dynamic target, string methodname, params dynamic[] paramters)
            {
                if (target != null && !string.IsNullOrWhiteSpace(methodname))
                {
                    string key = target.GetType().FullName + methodname;
                    MethodInvokeHandler invokeHandler;
                    if (m_cacheHandlers.ContainsKey(key))
                    {
                        invokeHandler = m_cacheHandlers[key];
                    }
                    else
                    {
                        var methodInfo = target.GetType().GetMethod(methodname);
                        if (methodInfo != null)
                        {
                            invokeHandler = GetMethodInvoker(methodInfo);
                            m_cacheHandlers[key] = invokeHandler;
                        }
                        else
                        {
                            throw new Exception(string.Format("{0} does not have method : {1}", target.GetType().FullName, methodname));
                        }
                    }
                    return invokeHandler.Invoke(target, paramters);

                }
                return null;
            }

            public static dynamic Invoker<T>(string methodname, params dynamic[] paramters)
            {
                Type type = typeof(T);
                string key = type.FullName + methodname;
                MethodInvokeHandler invokeHandler;
                if (m_cacheHandlers.ContainsKey(key))
                {
                    invokeHandler = m_cacheHandlers[key];
                }
                else
                {
                    var methodInfo = type.GetMethod(methodname);
                    if (methodInfo != null && methodInfo.IsStatic)
                    {
                        invokeHandler = GetMethodInvoker(methodInfo);
                        m_cacheHandlers[key] = invokeHandler;
                    }
                    else
                    {
                        throw new Exception(string.Format("{0} does not have static method : {1}", type.FullName, methodname));
                    }
                }
                return invokeHandler.Invoke(null, paramters);
            }

            public static dynamic Invoker<T>(dynamic target, string methodname, params dynamic[] paramters)
            {
                Type type = typeof(T);
                string key = type.FullName + methodname;
                MethodInvokeHandler invokeHandler;
                if (m_cacheHandlers.ContainsKey(key))
                {
                    invokeHandler = m_cacheHandlers[key];
                }
                else
                {
                    var methodInfo = type.GetMethod(methodname);
                    if (methodInfo != null && !methodInfo.IsStatic)
                    {
                        invokeHandler = GetMethodInvoker(methodInfo);
                        m_cacheHandlers[key] = invokeHandler;
                    }
                    else
                    {
                        throw new Exception(string.Format("{0} does not have method : {1}", type.FullName, methodname));
                    }
                }
                return invokeHandler.Invoke(target, paramters);
            }

            public delegate dynamic MethodInvokeHandler(dynamic target, params dynamic[] paramters);

            public static MethodInvokeHandler GetMethodInvoker(MethodInfo methodInfo)
            {
                DynamicMethod dynamicMethod = new DynamicMethod(string.Empty, typeof(object), new Type[] { typeof(object), typeof(object[]) }, methodInfo.DeclaringType.Module);
                ILGenerator il = dynamicMethod.GetILGenerator();
                ParameterInfo[] ps = methodInfo.GetParameters();
                Type[] paramTypes = new Type[ps.Length];
                for (int i = 0; i < paramTypes.Length; i++)
                {
                    if (ps[i].ParameterType.IsByRef)
                        paramTypes[i] = ps[i].ParameterType.GetElementType();
                    else
                        paramTypes[i] = ps[i].ParameterType;
                }
                LocalBuilder[] locals = new LocalBuilder[paramTypes.Length];

                for (int i = 0; i < paramTypes.Length; i++)
                {
                    locals[i] = il.DeclareLocal(paramTypes[i], true);
                }
                for (int i = 0; i < paramTypes.Length; i++)
                {
                    il.Emit(OpCodes.Ldarg_1);
                    EmitFastInt(il, i);
                    il.Emit(OpCodes.Ldelem_Ref);
                    EmitCastToReference(il, paramTypes[i]);
                    il.Emit(OpCodes.Stloc, locals[i]);
                }
                if (!methodInfo.IsStatic)
                {
                    il.Emit(OpCodes.Ldarg_0);
                }
                for (int i = 0; i < paramTypes.Length; i++)
                {
                    if (ps[i].ParameterType.IsByRef)
                        il.Emit(OpCodes.Ldloca_S, locals[i]);
                    else
                        il.Emit(OpCodes.Ldloc, locals[i]);
                }
                if (methodInfo.IsStatic)
                    il.EmitCall(OpCodes.Call, methodInfo, null);
                else
                    il.EmitCall(OpCodes.Callvirt, methodInfo, null);
                if (methodInfo.ReturnType == typeof(void))
                    il.Emit(OpCodes.Ldnull);
                else
                    EmitBoxIfNeeded(il, methodInfo.ReturnType);

                for (int i = 0; i < paramTypes.Length; i++)
                {
                    if (ps[i].ParameterType.IsByRef)
                    {
                        il.Emit(OpCodes.Ldarg_1);
                        EmitFastInt(il, i);
                        il.Emit(OpCodes.Ldloc, locals[i]);
                        if (locals[i].LocalType.IsValueType)
                            il.Emit(OpCodes.Box, locals[i].LocalType);
                        il.Emit(OpCodes.Stelem_Ref);
                    }
                }

                il.Emit(OpCodes.Ret);
                MethodInvokeHandler invoder = (MethodInvokeHandler)dynamicMethod.CreateDelegate(typeof(MethodInvokeHandler));
                return invoder;
            }

            #region Emit

            private static void EmitCastToReference(ILGenerator il, System.Type type)
            {
                if (type.IsValueType)
                {
                    il.Emit(OpCodes.Unbox_Any, type);
                }
                else
                {
                    il.Emit(OpCodes.Castclass, type);
                }
            }

            private static void EmitBoxIfNeeded(ILGenerator il, System.Type type)
            {
                if (type.IsValueType)
                {
                    il.Emit(OpCodes.Box, type);
                }
            }

            private static void EmitFastInt(ILGenerator il, int value)
            {
                switch (value)
                {
                    case -1:
                        il.Emit(OpCodes.Ldc_I4_M1);
                        return;
                    case 0:
                        il.Emit(OpCodes.Ldc_I4_0);
                        return;
                    case 1:
                        il.Emit(OpCodes.Ldc_I4_1);
                        return;
                    case 2:
                        il.Emit(OpCodes.Ldc_I4_2);
                        return;
                    case 3:
                        il.Emit(OpCodes.Ldc_I4_3);
                        return;
                    case 4:
                        il.Emit(OpCodes.Ldc_I4_4);
                        return;
                    case 5:
                        il.Emit(OpCodes.Ldc_I4_5);
                        return;
                    case 6:
                        il.Emit(OpCodes.Ldc_I4_6);
                        return;
                    case 7:
                        il.Emit(OpCodes.Ldc_I4_7);
                        return;
                    case 8:
                        il.Emit(OpCodes.Ldc_I4_8);
                        return;
                }

                if (value > -129 && value < 128)
                {
                    il.Emit(OpCodes.Ldc_I4_S, (SByte)value);
                }
                else
                {
                    il.Emit(OpCodes.Ldc_I4, value);
                }
            }

            #endregion
        }

        /// <summary>
        /// 自定义特性
        /// </summary>
        public static class Attribute
        {

            static readonly ConcurrentDictionary<string, dynamic> m_cacheCustomAttributes = new ConcurrentDictionary<string, dynamic>();

            /// <summary>
            /// 获取对象的自定义特性
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="p_Value"></param>
            /// <returns></returns>
            public static List<T> GetCustomAttribute<T>(dynamic p_Value) where T : System.Attribute
            {
                Type type = p_Value.GetType();
                string key = typeof(T).FullName + p_Value.ToString();
                if ((type.BaseType == typeof(MethodInfo) | type.BaseType == typeof(PropertyInfo) | type.BaseType == typeof(FieldInfo)) && p_Value.DeclaringType != null)
                {
                    key += p_Value.DeclaringType.FullName;
                }
                if (!m_cacheCustomAttributes.ContainsKey(key))
                {
                    if (p_Value is Type)
                    {
                        object[] customattributes = ((Type)p_Value).GetCustomAttributes(typeof(T), true);
                        if (customattributes.Length > 0)
                        {
                            m_cacheCustomAttributes[key] = customattributes.Select(p => (T)p).ToList();
                        }
                    }
                    else
                    {
                        if (type.BaseType == typeof(Enum))
                        {
                            var fild = type.GetField(p_Value.ToString());
                            if (fild.IsDefined(typeof(T), false))
                            {
                                object[] customattributes = fild.GetCustomAttributes(typeof(T), true);
                                if (customattributes.Length > 0)
                                {
                                    m_cacheCustomAttributes[key] = customattributes.Select(p => (T)p).ToList();
                                }
                            }
                        }
                        else if (type.BaseType == typeof(MethodInfo))
                        {
                            var method = ((MethodInfo)p_Value);
                            if (method.IsDefined(typeof(T), false))
                            {
                                object[] customattributes = method.GetCustomAttributes(typeof(T), true);
                                if (customattributes.Length > 0)
                                {
                                    m_cacheCustomAttributes[key] = customattributes.Select(p => (T)p).ToList();
                                }
                            }
                        }
                        else if (type.BaseType == typeof(PropertyInfo))
                        {
                            var property = ((PropertyInfo)p_Value);
                            if (property.IsDefined(typeof(T), false))
                            {
                                object[] customattributes = property.GetCustomAttributes(typeof(T), true);
                                if (customattributes.Length > 0)
                                {
                                    m_cacheCustomAttributes[key] = customattributes.Select(p => (T)p).ToList();
                                }
                            }
                        }
                        else if (type.BaseType == typeof(FieldInfo))
                        {
                            var fild = ((FieldInfo)p_Value);
                            if (fild.IsDefined(typeof(T), false))
                            {
                                object[] customattributes = fild.GetCustomAttributes(typeof(T), true);
                                if (customattributes.Length > 0)
                                {
                                    m_cacheCustomAttributes[key] = customattributes.Select(p => (T)p).ToList();
                                }
                            }
                        }
                        else if (type.BaseType == typeof(Object))
                        {
                            object[] customattributes = type.GetCustomAttributes(typeof(T), true);
                            if (customattributes.Length > 0)
                            {
                                m_cacheCustomAttributes[key] = customattributes.Select(p => (T)p).ToList();
                            }
                        }
                    }
                    if (!m_cacheCustomAttributes.ContainsKey(key))
                        m_cacheCustomAttributes[key] = new List<T>();
                }
                return m_cacheCustomAttributes[key];
            }
        }

    }
}