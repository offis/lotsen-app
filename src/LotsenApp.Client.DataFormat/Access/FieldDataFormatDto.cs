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
    public class FieldDataFormatDto
    {
        public string Id { get; }
        public string Name { get; }
        public string I18NKey { get; }
        public string Expression { get; }
        public DataTypeDataFormatDto Type { get; }

        public FieldDataFormatDto(DataField field, DataFieldDisplay fieldDisplay, Project project)
        {
            Id = field.Id;
            Name = field.Name;
            I18NKey = fieldDisplay?.I18NKey;
            Expression = field.Expression + ";" + fieldDisplay?.Expression;
            Type = new DataTypeDataFormatDto(project.DataDefinition.DataTypes.FirstOrDefault(d => d.Id == field.DataType), 
                project.DataDisplay?.DataTypes?.FirstOrDefault(d => d.Id == field.DataType));
        }
    }
}