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
using System.Threading.Tasks;
using LotsenApp.Client.Cryptography;
using LotsenApp.Client.Http;

namespace LotsenApp.Client.TanList
{
    public class TanListService
    {
        private readonly TanListContext _context;
        private readonly TanGenerator _generator;

        public TanListService(TanListContext context, TanGenerator generator)
        {
            _context = context;
            _generator = generator;
        }
        public async Task<string[]> CreateOrUpdateTanList(string userId)
        {
            var tans = new List<string>();
            for (var i = 0; i < 10; i++)
            {
                var newTan = _generator.GenerateTan();
                tans.Add(newTan);
            }
            var tanListModel = new TanListModel
            {
                UserId = userId,
                Tan1 = OneWayHashFunction.Hash(tans[0]),
                Tan1Used = false,
                Tan2 = OneWayHashFunction.Hash(tans[1]),
                Tan2Used = false,
                Tan3 = OneWayHashFunction.Hash(tans[2]),
                Tan3Used = false,
                Tan4 = OneWayHashFunction.Hash(tans[3]),
                Tan4Used = false,
                Tan5 = OneWayHashFunction.Hash(tans[4]),
                Tan5Used = false,
                Tan6 = OneWayHashFunction.Hash(tans[5]),
                Tan6Used = false,
                Tan7 = OneWayHashFunction.Hash(tans[6]),
                Tan7Used = false,
                Tan8 = OneWayHashFunction.Hash(tans[7]),
                Tan8Used = false,
                Tan9 = OneWayHashFunction.Hash(tans[8]),
                Tan9Used = false,
                Tan10 = OneWayHashFunction.Hash(tans[9]),
                Tan10Used = false,
            };
            var existingTans = await _context.TanList.FindAsync(userId);
            if (existingTans != null)
            {
                _context.Remove(existingTans);
            }

            await _context.TanList.AddAsync(tanListModel);

            await _context.SaveChangesAsync();

            return tans.ToArray();
        }

        public async Task<bool> HasTanList(string userId)
        {
            return (await _context.TanList.FindAsync(userId)) != null;
        }

        public async Task<(bool tanSuccess, bool tansLeft)> UseTan(string userId, string tan)
        {
            var tanInvalidated = false;
            var tanList = await _context.TanList.FindAsync(userId);

            if (tanList == null)
            {
                throw new BadRequestException();
            }

            if (OneWayHashFunction.Verify(tan, tanList.Tan1) && !tanList.Tan1Used)
            {
                tanList.Tan1 = "";
                tanList.Tan1Used = true;
                tanInvalidated = true;
            }

            if (OneWayHashFunction.Verify(tan, tanList.Tan2) && !tanList.Tan2Used)
            {
                tanList.Tan2 = "";
                tanList.Tan2Used = true;
                tanInvalidated = true;
            }

            if (OneWayHashFunction.Verify(tan, tanList.Tan3) && !tanList.Tan3Used)
            {
                tanList.Tan3 = "";
                tanList.Tan3Used = true;
                tanInvalidated = true;
            }

            if (OneWayHashFunction.Verify(tan, tanList.Tan4) && !tanList.Tan4Used)
            {
                tanList.Tan4 = "";
                tanList.Tan4Used = true;
                tanInvalidated = true;
            }

            if (OneWayHashFunction.Verify(tan, tanList.Tan5) && !tanList.Tan5Used)
            {
                tanList.Tan5 = "";
                tanList.Tan5Used = true;
                tanInvalidated = true;
            }

            if (OneWayHashFunction.Verify(tan, tanList.Tan6) && !tanList.Tan6Used)
            {
                tanList.Tan6 = "";
                tanList.Tan6Used = true;
                tanInvalidated = true;
            }

            if (OneWayHashFunction.Verify(tan, tanList.Tan7) && !tanList.Tan7Used)
            {
                tanList.Tan7 = "";
                tanList.Tan7Used = true;
                tanInvalidated = true;
            }

            if (OneWayHashFunction.Verify(tan, tanList.Tan8) && !tanList.Tan8Used)
            {
                tanList.Tan8 = "";
                tanList.Tan8Used = true;
                tanInvalidated = true;
            }

            if (OneWayHashFunction.Verify(tan, tanList.Tan9) && !tanList.Tan9Used)
            {
                tanList.Tan9 = "";
                tanList.Tan9Used = true;
                tanInvalidated = true;
            }

            if (OneWayHashFunction.Verify(tan, tanList.Tan10) && !tanList.Tan10Used)
            {
                tanList.Tan10 = "";
                tanList.Tan10Used = true;
                tanInvalidated = true;
            }

            _context.Update(tanList);

            await _context.SaveChangesAsync();

            return (tanInvalidated, !(tanList.Tan1Used && 
                                     tanList.Tan2Used &&
                                     tanList.Tan3Used &&
                                     tanList.Tan4Used &&
                                     tanList.Tan5Used &&
                                     tanList.Tan6Used &&
                                     tanList.Tan7Used &&
                                     tanList.Tan8Used &&
                                     tanList.Tan9Used &&
                                     tanList.Tan10Used
                                     ));
        }

    }
}