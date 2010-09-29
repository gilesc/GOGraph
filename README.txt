==Installation guide for Gene Ontology Shortest Path utility (compatible with VB6)

1. Ensure that you have Microsoft .NET framework v4.0 installed. (You can check by looking for a folder called "C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319").
2. Open a command prompt, navigate to this (GOGraph's) folder, and run "register.bat". Once you have run "register.bat", DON'T MOVE this folder without running "unregister.bat" first. (Then you'll have to run "register.bat" again after you move the folder).
3. Include the reference in your VB6 project by navigating to Project -> References, then looking for "GOGraph" in the "Available References" pane.  Check the "GOGraph" box.
4. Use the GOGraph library in your VB6 code as shown in the example src/ExampleUsage.vb6. Be sure to replace the path to the GO OBO v1.2 flat file with the correct path for your system.

==Steps for update, if a version of GOGraph is already installed:
1. Remove GOGraph from the references list in your current VB6 project. Save and close VB6.
2. Using an administrator command prompt, run "unregister.bat" in the current (old version) GOGraph directory.
3. Unzip and replace the old GOGraph folder with the attached new version.
4. Using an adminstrator prompt, run "register.bat"
5. Re-include GOGraph in the references to your VB6 project.

==Usage==
Currently, there are three functions, as follows:

GOGraph.ShortestPath(Int32 beginNode, Int32 endNode) => 
	returns array of integers representing the shortest path

GOGraph.FindAllAncestors(Int32 node) => 
	returns array of integers representing all ancestors up to the root node

GOGraph.IsAncestor(Int32 parent, Int32 child) => 
	returns boolean representing whether the first parameter (parent) is an ancestor of the second parameter, the child.


Notes:
-For your reference, the code for this utility is in GOGraph.vb.
-Currently in the graph, only "is_a" relationships are considered as links, not "intersection_of", "relationship", etc. If you need other relationship types added, just let me know.