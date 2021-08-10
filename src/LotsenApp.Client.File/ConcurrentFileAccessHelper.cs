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

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace LotsenApp.Client.File
{
    public static class ConcurrentFileAccessHelper
    {
        private static readonly IDictionary<string, ReaderWriterLockSlim> Locks = new ConcurrentDictionary<string, ReaderWriterLockSlim>();
        
        /// <summary>
        /// Get a semaphore that tracks access to this file.
        /// </summary>
        /// <param name="fileName">The name file name</param>
        /// <returns></returns>
        public static ReaderWriterLockSlim GetAccessor(string fileName)
        {
            if (Locks.ContainsKey(fileName))
            {
                return Locks[fileName];
            }

            var lockSlim = new ReaderWriterLockSlim();
            Locks.Add(fileName, lockSlim);
            return lockSlim;
        }

        public static void ReleaseAllLocks()
        {
            foreach (var readerWriterLockSlim in Locks)
            {
                var lockSlim = readerWriterLockSlim.Value;
                if (lockSlim.IsReadLockHeld)
                {
                    lockSlim.ExitReadLock();
                }

                if (lockSlim.IsWriteLockHeld)
                {
                    lockSlim.ExitWriteLock();
                }
            }
        }
    }
}