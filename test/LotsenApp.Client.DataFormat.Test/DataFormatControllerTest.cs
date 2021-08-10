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
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using LotsenApp.Client.DataFormat.Access;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LotsenApp.Client.DataFormat.Test
{
    [ExcludeFromCodeCoverage]
    public class DataFormatControllerTest
    {
        [Fact]
        public async Task ShouldCreateProject()
        {
            var storageMock = new Mock<IDataFormatStorage>();
            var logger = new Mock<ILogger<DataFormatController>>();
            storageMock.Setup(sto => sto.CreateProject(It.IsAny<Project>()).Result).Returns(true);
            var controller = new DataFormatController(storageMock.Object, logger.Object);

            var result = await controller.CreateProject(new Project());
            var statusResult = result as StatusCodeResult;
            Assert.Equal(200, statusResult?.StatusCode);
        }

        [Fact]
        public async Task ShouldNotCreateProject()
        {
            var storageMock = new Mock<IDataFormatStorage>();
            var logger = new Mock<ILogger<DataFormatController>>();
            storageMock.Setup(sto => sto.CreateProject(It.IsAny<Project>()).Result).Returns(false);
            var controller = new DataFormatController(storageMock.Object, logger.Object);

            var result = await controller.CreateProject(new Project());
            var statusResult = result as StatusCodeResult;
            Assert.Equal(202, statusResult?.StatusCode);
        }
        
        [Fact]
        public async Task ShouldReturnNotFound()
        {
            var storageMock = new Mock<IDataFormatStorage>();
            var logger = new Mock<ILogger<DataFormatController>>();
            storageMock.Setup(sto => sto.Projects).Returns(Array.Empty<Project>());
            var controller = new DataFormatController(storageMock.Object, logger.Object);

            var result = await controller.CreateI18N("", "", new I18NDto());
            var statusResult = result as StatusCodeResult;
            Assert.Equal(404, statusResult?.StatusCode);
        }
        
        [Fact]
        public async Task ShouldCreateI18N()
        {
            var storageMock = new Mock<IDataFormatStorage>();
            var logger = new Mock<ILogger<DataFormatController>>();
            storageMock.Setup(sto => sto.Projects).Returns(new[] {new Project
            {
                Id = "id"
            }});
            var controller = new DataFormatController(storageMock.Object, logger.Object);

            var result = await controller.CreateI18N("id", "", new I18NDto());
            var statusResult = result as StatusCodeResult;
            Assert.Equal(200, statusResult?.StatusCode);
        }
    }
}