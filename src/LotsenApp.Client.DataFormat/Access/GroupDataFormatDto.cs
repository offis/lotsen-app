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

using System.Linq;
using LotsenApp.Client.DataFormat.Definition;
using LotsenApp.Client.DataFormat.Display;

namespace LotsenApp.Client.DataFormat.Access
{
    public class GroupDataFormatDto
    {
        public string Id { get; }
        public string Name { get; }
        public string I18NKey { get; }
        public Cardinality Cardinality { get; }
        public GroupDataFormatDto[] Children { get; }
        public FieldDataFormatDto[] Fields { get; }

        public GroupDataFormatDto(Group group, GroupDisplay groupDisplay, Project project)
        {
            Id = group.Id;
            Name = group.Name;
            Cardinality = group.Cardinality;
            I18NKey = groupDisplay?.I18NKey;
            Fields = group.Fields.Select(f =>
                    new FieldDataFormatDto(project.DataDefinition.DataFields.FirstOrDefault(df => df.Id == f), 
                        project.DataDisplay.DataFields.FirstOrDefault(fd => fd.Id == f),
                        project))
                .OrderBy(f => groupDisplay?.DataFields.FirstOrDefault(dd => dd.Id == f.Id)?.Ordinal ?? 999)
                .ToArray();
            Children = group.Children.Select(g =>
                new GroupDataFormatDto(project.DataDefinition.Groups.FirstOrDefault(dg => dg.Id == g), 
                    project.DataDisplay?.Groups?.FirstOrDefault(gd => gd.Id == g), project))
                .OrderBy(g => groupDisplay?.Children.FirstOrDefault(gd => gd.Id == g.Id)?.Ordinal ?? 999)
                .ToArray();
        }
    }
}