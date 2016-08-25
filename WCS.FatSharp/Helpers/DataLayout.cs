//    Samsung Firmware Toolchain - A toolchain to edit and modify Samsung firmware files.
//    Copyright (C) 2010  Jason Couture
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see http://www.gnu.org/licenses/.


// Change Log
// 	<date> - <name/email> - Short description of changes
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Security.Permissions;

namespace WCS.FatSharp.Helpers
{
    public class DataLayout
    {
        //We require access to restricted members, Therefor
        //we ask for unrestricted access to metadata. This code
        //Will fail in a partially trusted environment where
        //Reflection Permission is limited in any way.
        /// <summary>
        /// Fill a type from a byte array.
        /// </summary>
        /// <typeparam name="T">Type to fill</typeparam>
        /// <param name="block">Byte array to read in the data from</param>
        /// <param name="toFill">Object to fill</param>
        [ReflectionPermission(SecurityAction.Demand, Unrestricted = true)]
        public static void Fill<T>(byte[] b, T toFill)
        {
            Fill<T>(b, toFill, 0);
        }
        [ReflectionPermission(SecurityAction.Demand, Unrestricted = true)]
        public static void Fill<T>(byte[] b, T toFill, int offset)
        {
            Fill<T>(b, toFill, offset, b.Length - offset);
        }
        [ReflectionPermission(SecurityAction.Demand, Unrestricted = true)]
        public static void Fill<T>(byte[] b, T toFill, int startoffset, int length)
        {
            byte[] block = new byte[length];
            Array.Copy(b, startoffset, block, 0, length);
            Type target = typeof(T);
            FieldInfo[] fis = target.GetFields(BindingFlags.Instance |
                BindingFlags.NonPublic |
                BindingFlags.Public);
            foreach (FieldInfo fi in fis)
            {
                Object[] attrs = fi.GetCustomAttributes(typeof(DataAttribute), false);
                foreach (object o in attrs)
                {
                    DataAttribute d = (DataAttribute)o;
                    FillSingle<T>(block, d, fi, toFill);
                }
            }
        }
        /// <summary>
        /// Dumps a type with DataAttribute members to a byte array.
        /// </summary>
        /// <typeparam name="T">Type to dump</typeparam>
        /// <param name="target">object to dump</param>
        /// <returns>Byte array filled with the required data.</returns>
        public static byte[] Dump<T>(T targetObject)
        {
            byte[] ret = new byte[FindRequiredArraySize(targetObject)];
            Type target = targetObject.GetType();
            FieldInfo[] fis = target.GetFields(BindingFlags.Instance |
                BindingFlags.NonPublic |
                BindingFlags.Public);
            foreach (FieldInfo fi in fis)
            {
                Object[] attrs = fi.GetCustomAttributes(typeof(DataAttribute), false);
                foreach (object a in attrs)
                {
                    DataAttribute d = (DataAttribute)a;
                    DumpField(fi, targetObject, d, ret);
                }
            }
            return ret;
        }
        static void DumpField(FieldInfo fi, object o, DataAttribute Attr, byte[] block)
        {
            byte[] ret;
            if (fi.FieldType == typeof(byte))
            {
                ret = new byte[1];
                ret[0] = (byte)fi.GetValue(o);
            }
            else if (fi.FieldType == typeof(uint))
            {
                ret = BitConverter.GetBytes((uint)fi.GetValue(o));
            }
            else if (fi.FieldType == typeof(int))
            {
                ret = BitConverter.GetBytes((int)fi.GetValue(o));
            }
            else if (fi.FieldType == typeof(short))
            {
                ret = BitConverter.GetBytes((short)fi.GetValue(o));
            }
            else if (fi.FieldType == typeof(ushort))
            {
                ret = BitConverter.GetBytes((ushort)fi.GetValue(o));
            }
            else if (fi.FieldType == typeof(string))
            {
                ret = new byte[Attr.Length];
                byte[] tmp = Attr.Encoding.GetBytes((string)fi.GetValue(o));
                if (tmp.Length > Attr.Length)
                    Array.Copy(tmp, 0, ret, 0, Attr.Length);
                else
                    Array.Copy(tmp, 0, ret, 0, tmp.Length);
            }
            else if (fi.FieldType == typeof(byte[]))
            {
                //This will choke if fieldValue.Length < Attr.Length
                byte[] val = (byte[])fi.GetValue(o);
                ret = new byte[Attr.Length];
                Array.Copy(val, 0, ret, 0, Attr.Length);
            }
            else
            {
                throw new NotSupportedException("FIXME: Unsupported Type: " + fi.FieldType.ToString());
            }
            Array.Copy(ret, 0, block, Attr.Offset, ret.Length);
        }
        static int FindRequiredArraySize(object o)
        {
            int largestLength = 0;
            Type target = o.GetType();
            FieldInfo[] fis = target.GetFields(BindingFlags.Instance |
                BindingFlags.NonPublic |
                BindingFlags.Public);
            foreach (FieldInfo fi in fis)
            {
                Object[] attrs = fi.GetCustomAttributes(typeof(DataAttribute), false);
                foreach (object a in attrs)
                {
                    DataAttribute d = (DataAttribute)a;
                    if ((d.Offset + d.Length) > largestLength)
                    {
                        largestLength = d.Offset + d.Length;
                    }
                }
            }
            return largestLength + 1;
        }
        [ReflectionPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        private static void FillSingle<T>(byte[] block, DataAttribute descriptor, FieldInfo fi, T target)
        {
            object val = GetFieldValue(block, descriptor, fi.FieldType);
            fi.SetValue(target, val);
        }
        [ReflectionPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        private static object GetFieldValue(byte[] block, DataAttribute d, Type t)
        {
            byte[] tmp = new byte[d.Length];
            unchecked
            {
                if (typeof(string) == t)
                {
                    if (d.Length <= 0)
                        throw new IndexOutOfRangeException("FIXME: For strings, Length must be > 0!");
                    //Okay. This should be relatively easy.
                    string s = d.Encoding.GetString(block, d.Offset, d.Length);
                    //Tada!
                    return s;
                }
                else if ((typeof(int) == t) || typeof(uint) == t)
                {
                    //Ignore d.Length here, its 4 bytes, Period.
                    uint ret = 0;
                    for (int i = 0; i < 4; i++)
                    {
                        ret = ret << 8;
                        ret += block[(3 - i) + d.Offset];
                    }
                    if (typeof(int) == t)
                        return (int)ret;
                    return ret;

                }
                else if (typeof(byte) == t)
                {
                    //Here we ignore length all-together.. lmao.
                    return block[d.Offset];
                }
                else if (typeof(byte[]) == t)
                {
                    //Ok, Piece of cake.
                    byte[] ret = new byte[d.Length];
                    Array.Copy(block, d.Offset, ret, 0, d.Length);
                    return ret;
                }
                else if ((typeof(ushort) == t) || (typeof(short) == t))
                {
                    //Again we ignore the length all-together. Always 2 bytes, Period.
                    ushort ret = 0;
                    for (int i = 0; i < 2; i++)
                    {
                        ret = (ushort)(ret << 8);
                        ret += block[(1 - i) + d.Offset];
                    }
                    if (typeof(short) == t)
                        return (short)ret;
                    return ret;
                }
            }
            //If we get here, It means we don't know this data type!
            throw new InvalidCastException(
                "FIXME: I don't know how to handle" +
                " this type! (" +
                t.ToString() +
                ")");
        }
    }
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class DataAttribute : Attribute
    {
        int mIndex = 0;
        int mLength = 0;
        Encoding mEncoding = Encoding.ASCII;
        public DataAttribute(int offset, int length, int enc)
        {
            mIndex = offset;
            mLength = length;
            mEncoding = Encoding.ASCII;
            switch (enc)
            {
                case 0:
                    mEncoding = Encoding.ASCII;
                    break;
                case 1:
                    mEncoding = Encoding.UTF8;
                    break;
                case 2:
                    mEncoding = Encoding.Unicode;
                    break;
                default:
                    mEncoding = Encoding.ASCII;
                    break;
            }
        }
        public DataAttribute(int offset, int length)
            : this(offset, length, 0)
        { }
        public DataAttribute(int offset)
            : this(offset, 1)
        { }

        public int Offset
        {
            get { return mIndex; }
        }

        public int Length
        {
            get { return mLength; }
        }

        public Encoding Encoding
        {
            get { return this.mEncoding; }
        }
    }
}
