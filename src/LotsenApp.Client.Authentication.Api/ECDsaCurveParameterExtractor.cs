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
using System.Linq;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace LotsenApp.Client.Authentication.Api
{
    public class ECDsaCurveParameterExtractor
    {
        public byte[] X { get; }
        public byte[] Y { get; }
        public ECDsaCurveParameterExtractor(string encodedPublicKey)
        {
            var decodedPublicKey = FromHexString(encodedPublicKey);
            // https://en.wikipedia.org/wiki/X.509
            // SubjectPublicKeyInfo enthält Informationen zum Verfahren (erster Teil) und den PublicKey (zweiter Teil).
            // Also muss von hinten angefangen werden, um den Public Key zu extrahieren
            var publicKeyInfo = decodedPublicKey.SkipWhile((c, i) => decodedPublicKey.Length - i > 133).ToArray();
            // Taken from https://www.scottbrady91.com/C-Sharp/JWT-Signing-using-ECDSA-in-dotnet-Core
            // Das erste byte ist der Tag des PublicKey
            // Die nächsten 32 byte sind der x-Wert
            // Die letzten 32 byte sind der y-Wert

            // Es liegt ein 512 byte private Key vor, also 1 byte Tag, 66 byte Q.X, 66 byte Q.Y

            X = publicKeyInfo.Skip(1).Take(66).ToArray();
            Y = publicKeyInfo.Skip(67).ToArray();
        }

        public SecurityKey CreateKey()
        {
            return new ECDsaSecurityKey(ECDsa.Create(new ECParameters
            {
                Curve = ECCurve.NamedCurves.nistP521,
                Q = new ECPoint
                {
                    X = X,
                    Y = Y
                }
            }));
        }

        public static SecurityKey FromECDsa(ECDsa ecdsa)
        {
            return new ECDsaSecurityKey(ecdsa);
        }

        public static byte[] FromHexString(string hex)
        {
            var numberChars = hex.Length;
            var hexAsBytes = new byte[numberChars / 2];
            for (var i = 0; i < numberChars; i += 2)
                hexAsBytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);

            return hexAsBytes;
        }
    }
}