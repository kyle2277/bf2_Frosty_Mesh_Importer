// ReflectionHelper.cs - FrostyResChunkImporter
// Contributors:
//      Copyright (C) 2020  Kyle Won
//      Copyright (C) 2020  Daniel Elam <dan@dandev.uk>
// This file is subject to the terms and conditions defined in the 'LICENSE' file.
// The following code is derived from Daniel Elam's bf2-sound-import project

using FrostySdk.Ebx;
using System;
using System.Reflection;

// <summary>
// Helper for getting and setting field values via Reflection
// </summary>

namespace FrostyMeshImporter
{
    public static class ReflectionHelper
    {
        private static FieldInfo GetFieldInfo(Type type, string fieldName)
        {
            FieldInfo fieldInfo;
            do
            {
                fieldInfo = type.GetField(fieldName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                type = type.BaseType;
            }
            while (fieldInfo == null && type != null);
            return fieldInfo;
        }

        public static T GetFieldValue<T>(this object obj, string fieldName)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            Type objType = obj.GetType();
            FieldInfo fieldInfo = GetFieldInfo(objType, fieldName);
            if (fieldInfo == null)
                throw new ArgumentOutOfRangeException(nameof(fieldName),
                    $"Couldn't find field {fieldName} in type {objType.FullName}");
            return (T)fieldInfo.GetValue(obj);
        }

        public static void SetFieldValue(this object obj, string fieldName, object val)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            Type objType = obj.GetType();
            FieldInfo fieldInfo = GetFieldInfo(objType, fieldName);
            if (fieldInfo == null)
                throw new ArgumentOutOfRangeException("fieldName",
                    string.Format("Couldn't find field {0} in type {1}", fieldName, objType.FullName));
            fieldInfo.SetValue(obj, val);
        }

        private static MethodInfo GetMethodInfo(Type type, string methodName)
        {
            MethodInfo methodInfo;
            do
            {
                methodInfo = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                type = type.BaseType;
            }
            while (methodInfo == null && type != null);
            return methodInfo;
        }

        public static void InvokeMethod(this object obj, string methodName, object[] parameters)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            Type objType = obj.GetType();
            MethodInfo mi = GetMethodInfo(objType, methodName);
            if (mi == null)
                throw new ArgumentOutOfRangeException(nameof(methodName),
                    $"Couldn't invoke method {methodName} in type {objType.FullName}");
            mi.Invoke(obj, parameters);
        }

        private static PropertyInfo GetPropertyInfo(Type type, string propertyName)
        {
            PropertyInfo propInfo;
            do
            {
                propInfo = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                type = type.BaseType;
            }
            while (propInfo == null && type != null);
            return propInfo;
        }

        public static T GetPropertyValue<T>(this object obj, string propertyName)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            Type objType = obj.GetType();
            PropertyInfo propInfo = GetPropertyInfo(objType, propertyName);
            if (propInfo == null)
                throw new ArgumentOutOfRangeException(nameof(propertyName),
                    $"Couldn't find property {propertyName} in type {objType.FullName}");
            return (T)propInfo.GetValue(obj);
        }
    }
}
