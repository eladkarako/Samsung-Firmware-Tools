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
// 	07/18/2010 - plaguethenet@gmail.com - Added GPL to all files.
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using MLFI.Interfaces;

namespace MLFI.Interfaces.Loader
{
    public class Loader
    {
        public static T[] LoadAndCreateInstance<T>(string AssemblyName)
        {
            Assembly asm = Assembly.LoadFile(AssemblyName);
            List<T> ret = new List<T>();
            Type[] types = asm.GetTypes();
            Type targetInterface = typeof(T);
            foreach(Type tpe in types)
            {
                if(targetInterface.IsAssignableFrom(tpe))
                {
                    try
                    {
                        T data = (T)System.Activator.CreateInstance(tpe);
                        ret.Add(data);
                    }
                    catch
                    {
                        //We ignore any exceptions, exceptions just cause
                        //The instance to not be created.
                    }
                }
            }
            return ret.ToArray();
        }
    }
}
