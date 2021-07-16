# GraphVR

GraphVR is an application to Visualize Graphs (SPARQL) as 3D Graphs in Virtual Reality.
The idea is to explore, analyze, and interact with the data in the graph in an intuitively.

GraphVR has been built in Unity and is able to connect to a SPARQL endpoint using [dotNetRDF](https://dotnetrdf.org/).
We got inspired by many diffferent tools to work with Linked data and to visualize Graphs.
Virtual Reality offers the user way more space to expand the graph, then a 2D computer screen.

We tried to reduce the ways to interact with the graph to a few simple operations:
- Getting some information about the current node
- Expanding the graph at a certain node using incoming or outgoing predicates.
- Deleting a node or collapsing those nodes around it that are not connected to the graph elsewhere.
- Filtering and slicing the graph to get only the relevant information
- Comparing different parts of the graph side by side (different visualisations)
- Building visual query pattern to form a SPARQL query.
- Adding new nodes and edges.
- Interacting with the graph (zoom, move, rotate) 


