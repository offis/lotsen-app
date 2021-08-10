// Copyright (c) 2021 OFFIS e.V.. All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 
// 1. Redistributions of source code must retain the above copyright notice, this
//    list of conditions and the following disclaimer.
//    
// 2. Redistributions in binary form must reproduce the above copyright notice,
//    this list of conditions and the following disclaimer in the documentation
//    and/or other materials provided with the distribution.
//    
// 3. Neither the name of the copyright holder nor the names of its contributors
//    may be used to endorse or promote products derived from this software without
//    specific prior written permission.
//    
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Linq;

namespace LotsenApp.Client.Common
{
    public static class Preconditions
    {
        public static T CheckNotNull<T>(T obj, string propName)
        {
            if (obj == null)
            {
                if (string.IsNullOrEmpty(propName))
                {
                    throw new ArgumentNullException();
                }
                throw new ArgumentNullException(propName);
            }
            return obj;
        }

        public static string CheckNotNullOrEmpty(string text, string propName)
        {
            CheckNotNull(text, propName);
            Check(text != "", propName);

            return text;
        }

        public static List<T> CheckNotEmpty<T>(List<T> list, string propName)
        {
            if (list == null)
            {
                if (string.IsNullOrEmpty(propName))
                {
                    throw new ArgumentNullException();
                }
                throw new ArgumentNullException(propName);
            }
            if (!list.Any())
            {
                if (string.IsNullOrEmpty(propName))
                {
                    throw new ArgumentNullException();
                }
                throw new ArgumentNullException(propName + " has no elements");
            }

            return list;
        }

        public static void Check(bool condition, string message)
        {
            if (!condition)
            {
                if (string.IsNullOrEmpty(message))
                {
                    throw new ArgumentException();
                }

                throw new ArgumentException(message);
            }
        }

        public static void CheckState(bool condition, string message)
        {
            if (!condition)
            {
                if (string.IsNullOrEmpty(message))
                {
                    throw new InvalidOperationException();
                }

                throw new InvalidOperationException(message);
            }
        }
    }
}
