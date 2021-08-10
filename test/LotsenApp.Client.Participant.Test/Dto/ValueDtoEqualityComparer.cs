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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using LotsenApp.Client.Participant.Dto;

namespace LotsenApp.Client.Participant.Test.Dto
{
    [ExcludeFromCodeCoverage]
    public class ValueDtoEqualityComparer: IEqualityComparer<DocumentValueDto>
    {
        public bool Equals(DocumentValueDto x, DocumentValueDto y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            var fieldDtoComparer = new FieldDtoEqualityComparer();
            var groupDtoComparer = new GroupDtoEqualityComparer();
            return x.Id == y.Id
                   && x.DocumentId == y.DocumentId
                   && x.Name == y.Name
                   && x.IsDelta == y.IsDelta
                   && x.Fields.All(f => fieldDtoComparer.Equals(f, y.Fields[x.Fields.IndexOf(f)]))
                   && x.Groups.All(g => groupDtoComparer.Equals(g, y.Groups[x.Groups.IndexOf(g)]));
        }

        public int GetHashCode(DocumentValueDto obj)
        {
            return HashCode.Combine(obj.Id, obj.DocumentId, obj.Name, obj.IsDelta, obj.Fields, obj.Groups);
        }
    }
    
    [ExcludeFromCodeCoverage]
    public class FieldDtoEqualityComparer: IEqualityComparer<FieldDto>
    {
        public bool Equals(FieldDto x, FieldDto y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.Id == y.Id && x.IsDelta == y.IsDelta && x.Value == y.Value && x.UseDisplay == y.UseDisplay;
        }

        public int GetHashCode(FieldDto obj)
        {
            return HashCode.Combine(obj.Id, obj.IsDelta, obj.Value, obj.UseDisplay);
        }
    }

    [ExcludeFromCodeCoverage]
    public class GroupDtoEqualityComparer : IEqualityComparer<GroupDto>
    {
        public bool Equals(GroupDto x, GroupDto y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            var groupDtoComparer = new GroupDtoEqualityComparer();
            var fieldComparer = new FieldDtoEqualityComparer();
            return x.Id == y.Id 
                   && x.GroupId == y.GroupId 
                   && x.IsDelta == y.IsDelta 
                   && x.Fields.All(f => fieldComparer.Equals(f, y.Fields[x.Fields.IndexOf(f)]))
                   && x.Children.All(g => groupDtoComparer.Equals(g, y.Children[x.Children.IndexOf(g)]));
        }

        public int GetHashCode(GroupDto obj)
        {
            return HashCode.Combine(obj.Id, obj.GroupId, obj.IsDelta, obj.Children, obj.Fields);
        }
    }
}