Imports System.IO
Imports QuickGraph
Imports QuickGraph.Algorithms

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
    Dim graph As UndirectedGraph(Of Int32, UndirectedEdge(Of Int32))

    Public Sub InitializeGraph(ByVal goPath As String)
        graph = New UndirectedGraph(Of Int32, UndirectedEdge(Of Int32))

        Dim line As String
        Dim currentTerm, parentTerm As Int32

        Using reader As StreamReader = New StreamReader(goPath)
            While Not reader.EndOfStream
                line = reader.ReadLine
                If line.StartsWith("[Typedef]") Then
                    reader.ReadToEnd()
                ElseIf line.StartsWith("id:") Then
                    currentTerm = Int32.Parse(line.Substring(7, 7))
                    graph.AddVertex(currentTerm)
                ElseIf line.StartsWith("is_a:") Then
                    parentTerm = Int32.Parse(line.Substring(10, 7))
                    graph.AddVertex(parentTerm)
                    graph.AddEdge(New UndirectedEdge(Of Int32)(currentTerm, parentTerm))
                End If

            End While
        End Using

    End Sub

    Dim costFn As Func(Of UndirectedEdge(Of Int32), Double) = Function() 1
    Public Function ShortestPath(ByVal startNode As Int32, ByVal endNode As Int32) As Int32()
        Dim path As IEnumerable(Of UndirectedEdge(Of Int32))

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


