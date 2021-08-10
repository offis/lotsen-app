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
using Xunit;

namespace LotsenApp.Client.Plugin.Test.Graph
{
    [ExcludeFromCodeCoverage]
    public class AdjacencyListTest
    {
        [Fact]
        public void ShouldSetInitialValues()
        {
            var list = new AdjacencyList<string, string>();
            
            Assert.Empty(list.Nodes);
            Assert.Empty(list.Edges);
        }
        
        [Fact]
        public void ShouldAddNode()
        {
            var list = new AdjacencyList<string, string>();

            list.AddNode(new Node<string>(0, "value"));
            
            Assert.Single(list.Nodes);
            Assert.Empty(list.Edges);
        }
        
        [Fact]
        public void ShouldAddNodeByValue()
        {
            var list = new AdjacencyList<string, string>();

            list.AddNode("value");
            
            Assert.Single(list.Nodes);
            Assert.Empty(list.Edges);
        }
        
        [Fact]
        public void ShouldAddNodeWithoutDuplicate()
        {
            var list = new AdjacencyList<string, string>();

            list.AddNodeNoDuplicateValue("value");
            list.AddNodeNoDuplicateValue("value");
            
            Assert.Single(list.Nodes);
            Assert.Empty(list.Edges);
        }
        
        [Fact]
        public void ShouldGetNodeLabel()
        {
            var list = new AdjacencyList<string, string>();

            list.AddNode(new Node<string>(0, "value"));

            var label = list.GetLabel(0);
            Assert.Equal("value", label);
        }
        
        [Fact]
        public void ShouldRemoveNodeById()
        {
            var list = new AdjacencyList<string, string>();
            var node = new Node<string>(0, "value");
            list.AddNode(node);

            list.RemoveNode(0);
            Assert.Empty(list.Nodes);
        }
        
        [Fact]
        public void ShouldRemoveEdgesOnNodeRemoval()
        {
            var list = new AdjacencyList<string, string>();
            var node = new Node<string>(0, "value");
            list.AddNode(node);
            list.AddEdge(0, 0, "");

            list.RemoveNode(0);
            Assert.Empty(list.Edges);
        }
        
        [Fact]
        public void ShouldRemoveInComingEdgesOnNodeRemoval()
        {
            var list = new AdjacencyList<string, string>();
            var node1 = list.AddNode("value");
            var node2 = list.AddNode("value");
            list.AddEdge(node2.Index, node1.Index, "");

            list.RemoveNode(0);
            Assert.Empty(list.Edges);
        }
        
        [Fact]
        public void ShouldRemoveNodeByNode()
        {
            var list = new AdjacencyList<string, string>();
            var node = new Node<string>(0, "value");
            list.AddNode(node);

            list.RemoveNode(node);
            Assert.Empty(list.Nodes);
        }
        
        [Fact]
        public void ShouldRemoveNodeByValue()
        {
            var list = new AdjacencyList<string, string>();
            var node = new Node<string>(0, "value");
            list.AddNode(node);

            list.RemoveNode("value");
            Assert.Empty(list.Nodes);
        }
        
        [Fact]
        public void ShouldThrowOnDuplicateNode()
        {
            var list = new AdjacencyList<string, string>();
            var node = new Node<string>(0, "value");
            list.AddNode(node);
            Assert.Throws<NodeWithSameIndexAlreadyExistsException>(() => list.AddNode(node));
        }
        
        [Fact]
        public void ShouldThrowOnUnknownNode()
        {
            var list = new AdjacencyList<string, string>();
            Assert.Throws<InvalidOperationException>(() => list.GetLabel(0));
        }
        
        [Fact]
        public void ShouldAddEdgeByValues()
        {
            var list = new AdjacencyList<string, string>();
            var node1 = list.AddNode("value1");
            var node2 = list.AddNode("value2");
            list.AddEdge(node1.Value, node2.Value, "");
            var edges = list.Edges;
            Assert.Single(edges);
            var edge = edges[0];
            Assert.Equal(node1.Index, edge.StartIndex);
            Assert.Equal(node2.Index, edge.EndIndex);
            Assert.Equal("", edge.Value);
        }
        
        [Fact]
        public void ShouldThrowOnUnknownStartValue()
        {
            var list = new AdjacencyList<string, string>();
            var node2 = list.AddNode("value2");
            Assert.Throws<NoNodeWithValueException>(() => list.AddEdge("value", node2.Value, ""));
        }
        
        [Fact]
        public void ShouldThrowOnUnknownEndValue()
        {
            var list = new AdjacencyList<string, string>();
            var node1 = list.AddNode("value1");
            Assert.Throws<NoNodeWithValueException>(() => list.AddEdge(node1.Value, "value", ""));
        }
        
        [Fact]
        public void ShouldAddEdge()
        {
            var list = new AdjacencyList<string, string>();
            var node1 = list.AddNode("value1");
            var node2 = list.AddNode("value2");
            var edge = new Edge<string>(node1.Index, node2.Index, "");
            list.AddEdge(edge);

            Assert.Single(list.Edges);
        }
        
        [Fact]
        public void ShouldCalculateIndegreeWithNodeId()
        {
            var list = new AdjacencyList<string, string>();
            var node1 = list.AddNode("value1");
            var node2 = list.AddNode("value2");
            var edge = new Edge<string>(node1.Index, node2.Index, "");
            list.AddEdge(edge);
            var node1Indegree = list.Indegree(node1.Index);
            var node2Indegree = list.Indegree(node2);
            Assert.Equal(0, node1Indegree);
            Assert.Equal(1, node2Indegree);
        }
        
        [Fact]
        public void ShouldRemoveOutGoingEdges()
        {
            var list = new AdjacencyList<string, string>();
            var node1 = list.AddNode("value1");
            var node2 = list.AddNode("value2");
            var edge = new Edge<string>(node1.Index, node2.Index, "");
            list.AddEdge(edge);
            list.RemoveOutgoingEdges(node2);
            Assert.Single(list.Edges);
        }
        
        [Fact]
        public void ShouldRemoveOutGoingEdgesWithExistingEdges()
        {
            var list = new AdjacencyList<string, string>();
            var node1 = list.AddNode("value1");
            var node2 = list.AddNode("value2");
            var edge = new Edge<string>(node1.Index, node2.Index, "");
            list.AddEdge(edge);
            list.RemoveOutgoingEdges(node1);
            Assert.Empty(list.Edges);
        }
    }
}