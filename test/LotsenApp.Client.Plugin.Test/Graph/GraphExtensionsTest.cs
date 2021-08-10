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
using System.Linq;
using LotsenApp.Client.Plugin.Graph;
using Moq;
using Xunit;

namespace LotsenApp.Client.Plugin.Test.Graph
{
    [ExcludeFromCodeCoverage]
    public class GraphExtensionsTest
    {
        [Fact]
        public void ShouldReturnNodesWithSpecificIndegree()
        {
            var digraphMock = new Mock<IDiGraph<string, string>>();
            var node1 = new Node<string>(0, "value");
            var node2 = new Node<string>(1, "value");
            digraphMock
                .Setup(g => g.Nodes)
                .Returns(new[] {node1, node2});
            digraphMock.Setup(g => g.Indegree(node1)).Returns(1);
            digraphMock.Setup(g => g.Indegree(node2)).Returns(0);

            Assert.Single(digraphMock.Object.NodesWithIndegree(1));
        }
        
        [Fact]
        public void ShouldReturnTopologicalSortingOfNodes()
        {
            var digraphMock = new Mock<IDiGraph<string, string>>();
            var node1 = new Node<string>(0, "value");
            var node2 = new Node<string>(1, "value");
            var node3 = new Node<string>(2, "value");
            var edge1 = new Edge<string>(node1.Index, node2.Index, "");
            var edge2 = new Edge<string>(node1.Index, node3.Index, "");
            var edge3 = new Edge<string>(node2.Index, node3.Index, "");
            digraphMock
                .Setup(g => g.Nodes)
                .Returns(new[] {node1, node2, node3});
            digraphMock
                .Setup(g => g.Edges)
                .Returns(new[] {edge1, edge2, edge3});
            var topSort = digraphMock.Object.TopologicalSort().ToList();
            Assert.Equal(3, topSort.Count);
            Assert.Equal(node3, topSort[0]);
            Assert.Equal(node2, topSort[1]);
            Assert.Equal(node1, topSort[2]);
        }
        
        [Fact]
        public void ShouldThrowWithTopologicalAndCircle()
        {
            var digraphMock = new Mock<IDiGraph<string, string>>();
            var node1 = new Node<string>(0, "value");
            var node2 = new Node<string>(1, "value");
            var node3 = new Node<string>(2, "value");
            var edge1 = new Edge<string>(node1.Index, node2.Index, "");
            var edge2 = new Edge<string>(node3.Index, node1.Index, "");
            var edge3 = new Edge<string>(node2.Index, node3.Index, "");
            digraphMock
                .Setup(g => g.Nodes)
                .Returns(new[] {node1, node2, node3});
            digraphMock
                .Setup(g => g.Edges)
                .Returns(new[] {edge1, edge2, edge3});
            Assert.Throws<Exception>(() => digraphMock.Object.TopologicalSort().ToList());
        }
    }
}