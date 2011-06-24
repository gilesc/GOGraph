Imports System.IO
Imports QuickGraph
Imports QuickGraph.Algorithms
Imports QuickGraph.Algorithms.Search
Imports QuickGraph.Algorithms.Observers
Imports QuickGraph.Algorithms.AlgorithmExtensions

Module GOGraph
    Dim graphs As New Dictionary(Of String, AdjacencyGraph(Of Int32, Edge(Of Int32)))
    Dim ontology As New Dictionary(Of Int32, String)
    Dim undirectedGraphs As New Dictionary(Of String, UndirectedGraph(Of Int32, UndirectedEdge(Of Int32)))
    Dim canonicalNodes As New Dictionary(Of Int32, Int32)

    Public Sub InitializeGraph(ByVal goPath As String)
        Dim graph As AdjacencyGraph(Of Int32, Edge(Of Int32))
        Dim line, goNamespace As String
        Dim currentTerm, parentTerm, altId As Int32

        'Initialize directed graphs
        Using reader As StreamReader = New StreamReader(goPath)
            While Not reader.EndOfStream
                line = reader.ReadLine
                If line.StartsWith("[Typedef]") Then
                    reader.ReadToEnd()
                ElseIf line.StartsWith("id:") Then
                    currentTerm = Int32.Parse(line.Substring(7, 7))
                    canonicalNodes.Add(currentTerm, currentTerm)
                ElseIf line.StartsWith("alt_id:") Then
                    altId = Int32.Parse(line.Substring(11, 7))
                    canonicalNodes.Add(altId, currentTerm)
                ElseIf line.StartsWith("namespace:") Then
                    goNamespace = line.Substring(11)
                    ontology.Add(currentTerm, goNamespace)
                ElseIf line.StartsWith("consider:") Then
                    If Not canonicalNodes.ContainsKey(currentTerm) Then
                        canonicalNodes.Add(currentTerm, line.Substring(InStr(line, "GO:") + 3, 7))
                    End If
                ElseIf line.StartsWith("is_a:") Or line.StartsWith("intersection_of") Or line.StartsWith("relationship:") Then
                    If Not graphs.ContainsKey(goNamespace) Then
                        graphs.Add(goNamespace, New AdjacencyGraph(Of Int32, Edge(Of Int32)))
                    End If
                    graph = graphs.Item(goNamespace)

                    graph.AddVertex(currentTerm)

                    parentTerm = Int32.Parse(line.Substring(InStr(line, "GO:") + 2, 7))
                    graph.AddVertex(parentTerm)
                    graph.AddEdge(New Edge(Of Int32)(currentTerm, parentTerm))
                End If
            End While
        End Using

        'Initialize undirected graphs
        For Each ns In graphs.Keys
            Dim g = graphs.Item(ns)
            Dim newGraph As New UndirectedGraph(Of Int32, UndirectedEdge(Of Int32))
            For Each e In g.Edges
                newGraph.AddVertex(e.Source)
                newGraph.AddVertex(e.Target)
                newGraph.AddEdge(New UndirectedEdge(Of Int32)(e.Source, e.Target))
            Next
            undirectedGraphs.Add(ns, newGraph)
        Next
    End Sub

    Public Function FindAllAncestors(ByVal node As Int32)
        node = GetCanonicalNode(node)
        Dim result = FindAncestorSet(node)
        Dim outArr(result.Count - 1) As Int32
        result.CopyTo(outArr)
        Return outArr
    End Function

    Public Function IsAncestor(ByVal parent As Int32, ByVal child As Int32)
        parent = GetCanonicalNode(parent)
        child = GetCanonicalNode(child)
        'Return FindAncestorSet(child).Contains(parent)
        Return FindAncestorSet(parent).Contains(child)
    End Function

    Private Function FindAncestorSet(ByVal node As Int32) As HashSet(Of Int32)
        Dim graph As AdjacencyGraph(Of Int32, Edge(Of Int32))
        For Each g In graphs.Values
            If g.ContainsVertex(node) Then graph = g
        Next
        If graph Is Nothing Then Return New HashSet(Of Int32)
        Return FindAncestorSet(graph, node)
    End Function

    Private Function FindAncestorSet(ByRef graph As AdjacencyGraph(Of Int32, Edge(Of Int32)), ByVal node As Int32) As HashSet(Of Int32)
        Dim result As New HashSet(Of Int32)
        For Each outEdge In graph.OutEdges(node)
            result.Add(outEdge.Target)
            For Each i In FindAncestorSet(graph, outEdge.Target)
                result.Add(i)
            Next
        Next
        Return result
    End Function

    Private Function GetCanonicalNode(ByVal node As Int32) As Int32
        Dim result As Integer
        canonicalNodes.TryGetValue(node, result)
        If result = 0 Then
            Throw New Exception("Node GO:" + node.ToString + " is not in the GO graph")
        End If
        Return result
    End Function

    Dim costFn As Func(Of UndirectedEdge(Of Int32), Double) = Function() 1
    Public Function ShortestPath(ByVal startNode As Int32, ByVal endNode As Int32) As Int32()
        startNode = GetCanonicalNode(startNode)
        endNode = GetCanonicalNode(endNode)

        Dim path As IEnumerable(Of UndirectedEdge(Of Int32))

        'Choose appropriate graph (or return nothing if terms aren't in same namespace)
        If Not ontology.Item(startNode) = ontology.Item(endNode) Then
            Return New Int32() {}
        End If

        Dim graph As UndirectedGraph(Of Int32, UndirectedEdge(Of Int32))
        For Each g In undirectedGraphs.Values
            If g.ContainsVertex(startNode) And g.ContainsVertex(endNode) Then
                graph = g
            End If
        Next
        If graph Is Nothing Then
            Return New Int32() {}
        End If

        'Find shortest path
        Dim nodes As New List(Of Int32)
        If (graph.ShortestPathsDijkstra(costFn, startNode)(endNode, path)) Then
            nodes.Add(startNode)
            For Each e As UndirectedEdge(Of Int32) In path
                If (e.Source = nodes.Item(nodes.Count - 1)) Then
                    nodes.Add(e.Target)
                Else
                    nodes.Add(e.Source)
                End If
            Next
        End If
        Return nodes.ToArray()
    End Function
End Module
