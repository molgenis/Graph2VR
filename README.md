# GraphVR

GraphVR is an application to Visualize Graphs (SPARQL) as 3D Graphs in Virtual Reality.
The idea is to explore, analyze, and interact with the data in the graph in an intuitively.

GraphVR has been built in Unity and is able to connect to a SPARQL endpoint using [dotNetRDF](https://dotnetrdf.org/).
We got inspired by many diffferent tools to work with Linked data and to visualize Graphs.
Virtual Reality offers the user way more space to expand the graph, then a 2D computer screen.

We tried to reduce the ways to interact with the graph to a few simple operations:
- Getting some information about the current node
- Expanding the graph at a certain node using a specific incoming or outgoing predicate.
- Declaring a node or edge in the graph as variable node, to build a query pattern for a SPARQL query.
- If you can expand a node, it has to be possible to reduce the amount of nodes again or to remove a certain node
- Beeing able to filter helps to slice the graph to the relevant parts
- Besides of the pure visualisation GraphVR is also able to create new nodes and edges.









