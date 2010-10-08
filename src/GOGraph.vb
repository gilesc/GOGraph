Imports System.IO
Imports QuickGraph
Imports QuickGraph.Algorithms
Imports QuickGraph.Algorithms.Search
Imports QuickGraph.Algorithms.Observers
Imports QuickGraph.Algorithms.AlgorithmExtensions

<ComClass(GOGraph.ClassId, GOGraph.InterfaceId, GOGraph.EventsId)> _
Public Class GOGraph

#Region "COM GUIDs"
    ' These  GUIDs provide the COM identity for this class 
    ' and its COM interfaces. If you change them, existing 
    ' clients will no longer be able to access the class.
    Public Const ClassId As String = "67af4e82-5b2e-4171-84b7-e25c38edc99d"
    Public Const InterfaceId As String = "11b7397c-855f-4ac0-ac33-d69257c72d11"
    Public Const EventsId As String = "3a982032-999e-49e3-97d3-ece3b94982b7"
#End Region

    ' A creatable COM class must have a Public Sub New() 
    ' with no parameters, otherwise, the class will not be 
    ' registered in the COM registry and cannot be created 
    ' via CreateObject.    
    Public Sub New()
        MyBase.New()
    End Sub
    Dim graphs As New Dictionary(Of String, AdjacencyGraph(Of Int32, Edge(Of Int32)))
    Dim undirectedGraphs As New Dictionary(Of String, UndirectedGraph(Of Int32, UndirectedEdge(Of Int32)))

    Public Sub InitializeGraph(ByVal goPath As String)
        Dim graph As AdjacencyGraph(Of Int32, Edge(Of Int32))
        Dim line, goNamespace As String
        Dim currentTerm, parentTerm As Int32

        'Initialize directed graphs
        Using reader As StreamReader = New StreamReader(goPath)
            While Not reader.EndOfStream
                line = reader.ReadLine
                If line.StartsWith("[Typedef]") Then
                    reader.ReadToEnd()
                ElseIf line.StartsWith("id:") Then
                    currentTerm = Int32.Parse(line.Substring(7, 7))
                ElseIf line.StartsWith("namespace:") Then
                    goNamespace = line.Substring(11)
                ElseIf line.StartsWith("is_a:") Then
                    If Not graphs.ContainsKey(goNamespace) Then
                        graphs.Add(goNamespace, New AdjacencyGraph(Of Int32, Edge(Of Int32)))
                    End If
                    graph = graphs.Item(goNamespace)

                    graph.AddVertex(currentTerm)
                    parentTerm = Int32.Parse(line.Substring(10, 7))
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
        Dim result = FindAncestorSet(node)
        Dim outArr(result.Count - 1) As Int32
        result.CopyTo(outArr)
        Return outArr
    End Function

    Public Function IsAncestor(ByVal parent As Int32, ByVal child As Int32)
        Return FindAncestorSet(child).Contains(parent)
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

    Dim costFn As Func(Of UndirectedEdge(Of Int32), Double) = Function() 1
    Public Function ShortestPath(ByVal startNode As Int32, ByVal endNode As Int32) As Int32()
        Dim path As IEnumerable(Of UndirectedEdge(Of Int32))

        'Choose appropriate graph (or return nothing if terms aren't in same namespace)
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
End Class

Module Test
    Dim graph As New GOGraph()

    Sub PrintPath(ByVal startNode As Int32, ByVal endNode As Int32)
        For Each node In graph.ShortestPath(startNode, endNode)
            Console.WriteLine(node)
        Next
        Console.WriteLine("-----")
    End Sub

    Sub main()
        graph.InitializeGraph("C:/data/gene_ontology_ext.obo")
        PrintPath(8285, 8150)
        PrintPath(19568, 8150)
        For Each i In graph.FindAllAncestors(8285)
            Console.WriteLine(i)
        Next
        Console.WriteLine("-----")
        Console.WriteLine(graph.IsAncestor(8150, 8285))
        Console.WriteLine(graph.IsAncestor(8151, 8285))
        Console.ReadKey()
    End Sub
End Module