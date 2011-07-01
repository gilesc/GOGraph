# GOGraph

A VB.NET based library for traversing graph of Gene Ontology terms, especially for finding the shortest paths between two given nodes in the graph.

## Incorporating the library

This library is currently not provided as a binary (hopefully we'll fix this in the future).  For now, include the source (GOGraph.vb) in your code, along with the provided dependency QuickGraph.dll.  (QuickGraph)(http://quickgraph.codeplex.com/) serves as the backend, providing algorithms to find shortest paths, etc.

Also required to use the library is the latest version of the Gene Ontology, which can be found at:
http://www.geneontology.org/ontology/obo_format_1_2/gene_ontology_ext.obo


## Usage
First, initiate the GOGraph object with:

       Dim graph As New GOGraph()
       graph.InitializeGraph(pathToGoFile)

Next, use any of the following functions:

GOGraph.ShortestPath(Int32 beginNode, Int32 endNode) => 
	returns array of integers representing the shortest path

GOGraph.FindAllAncestors(Int32 node) => 
	returns array of integers representing all ancestors up to the root node

GOGraph.IsAncestor(Int32 parent, Int32 child) => 
	returns boolean representing whether the first parameter (parent) is an ancestor of the second parameter, the child.

## Caveats

The following relationship types are valid links (and thus can be used to find paths) between GO terms:

* is_a
* intersection_of
* relationship

Perhaps this should be configurable?

## License
Copyright 2010-2011 Oklahoma Medical Research Foundation.
Distributed under the Eclipse Public License.