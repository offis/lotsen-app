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

namespace LotsenApp.Client.Plugin.Graph
{
    public class AdjacencyList<TN,TE>: IDiGraph<TN, TE>
    {
        private int _currentIndex;
        private readonly List<Node<TN>> _nodes = new List<Node<TN>>();
        private readonly Dictionary<int, List<(int index, TE value)>> _edges = new Dictionary<int, List<(int index, TE value)>>();

        public IReadOnlyList<Node<TN>> Nodes => _nodes;
        public IReadOnlyList<Edge<TE>> Edges => _edges.SelectMany(kv => kv.Value, (kv, v) => new Edge<TE>(kv.Key, v.index, v.value)).ToList();

        public Node<TN> AddNode(TN value)
        {
            var newNode = new Node<TN>(_currentIndex++, value);
            _nodes.Add(newNode);
            return newNode;
        }

        public TN GetLabel(int nodeId)
        {
            return _nodes.First(n => n.Index == nodeId).Value;
        }

        public void AddNode(Node<TN> node)
        {
            if (_nodes.Any(n => n.Index == node.Index))
            {
                throw new NodeWithSameIndexAlreadyExistsException($"A node with the index {node.Index} already exists.");
            }

            _nodes.Add(node);
        }

        public void RemoveNode(int index)
        {
            _nodes.RemoveAll(n => n.Index == index);
            _edges.Remove(index);
            foreach (var keyValuePair in _edges)
            {
                keyValuePair.Value.RemoveAll(e => e.index == index);
            }
        }

        public void RemoveNode(Node<TN> node)
        {
            RemoveNode(node.Index);
        }

        public void RemoveNode(TN value)
        {
            RemoveNode(FindNode(value));
        }

        public Node<TN> FindNode(TN value)
        {
            return _nodes.First(n => n.Value.Equals(value));
        }
        public void AddNodeNoDuplicateValue(TN value)
        {
            if (_nodes.Any(n => n.Value.Equals(value)))
            {
                return;
            }

            AddNode(value);
        }

        public Edge<TE> AddEdge(int startIndex, int endIndex, TE value)
        {
            var newEdge = new Edge<TE>(startIndex, endIndex, value);

            if (!_edges.ContainsKey(startIndex))
            {
                _edges.Add(startIndex, new List<(int index, TE value)>());
            }
            _edges[startIndex].Add((endIndex, value));

            return newEdge;
        }

        public void AddEdge(TN startValue, TN endValue, TE value)
        {
            int startIndex;
            int endIndex;
            try
            {
                startIndex = _nodes.First(n => n.Value.Equals(startValue)).Index;
            }
            catch (Exception)
            {
                throw new NoNodeWithValueException($"No node with the value {startValue} could be found");
            }
            try
            {
                endIndex = _nodes.First(n => n.Value.Equals(endValue)).Index;
            }
            catch (Exception)
            {
                throw new NoNodeWithValueException($"No node with the value {endValue} could be found");
            }

            AddEdge(startIndex, endIndex, value);
        }

        public void AddEdge(Edge<TE> edge)
        {
            AddEdge(edge.StartIndex, edge.EndIndex, edge.Value);
        }

        public int Indegree(int index)
        {
            return _edges.Aggregate(0, (current, edgeList) => current + edgeList.Value.Count(e => e.index == index));
        }

        public int Indegree(Node<TN> node)
        {
            return Indegree(node.Index);
        }

        public void RemoveOutgoingEdges(int index)
        {
            _edges.Remove(index);
        }

        public void RemoveOutgoingEdges(Node<TN> node)
        {
            RemoveOutgoingEdges(node.Index);
        }

    }
}