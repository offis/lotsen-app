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

namespace LotsenApp.Client.Plugin.Graph
{
    public static class GraphExtensions
    {
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public static IEnumerable<Node<TN>> TopologicalSort<TN, TE>(this IDiGraph<TN, TE> digraph)
        {
            var nodes = digraph.Nodes;
            var edges = digraph.Edges.ToList();
            // Kahn's algorithm
            var L = new List<Node<TN>>();
            var S = nodes.Where(n => !edges.Select(e => e.StartIndex).Contains(n.Index)).ToList();
            while (S.Any())
            {
                var node = S.First();
                S.Remove(node);
                L.Add(node);
                foreach (var edge in edges.Where(e => e.EndIndex == node.Index).ToList())
                {
                    edges.Remove(edge);
                    if (edges.All(remainingEdges => remainingEdges.StartIndex != edge.StartIndex))
                    {
                        S.Add(nodes.First(n => n.Index == edge.StartIndex));
                    }
                }
            }

            if (edges.Any())
            {
                throw new Exception("The digraph contains a circle.");
            }

            return L;
        }

        public static IEnumerable<Node<TN>> NodesWithIndegree<TN, TE>(this IDiGraph<TN, TE> digraph, int indegree)
        {
            return digraph.Nodes.Where(node => digraph.Indegree(node) == indegree);
        }
    }
}