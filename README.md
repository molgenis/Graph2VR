# Graph2VR

Graph2VR is a PhD project, a prototype for a VR application to Visualize Graphs (SPARQL) as 3D Graphs in Virtual Reality.
The idea is to explore, analyze, and interact with the data in the graph using gesture control.

Graph2VR has been built in Unity and is able to connect to a SPARQL endpoint using [dotNetRDF](https://dotnetrdf.org/).
We got inspired by many different tools to work with Linked data and to visualize Graphs.
Virtual Reality offers the user way more space to expand the graph, then a 2D computer screen.

We tried to reduce the ways to interact with the graph to a few simple operations:
- Getting some information about the current node
- Expanding the graph at a certain node using incoming or outgoing predicates.
- Deleting a node or collapsing those nodes around it that are not connected to the graph elsewhere.
- Comparing different parts of the graph side by side (different visualisations)
- Building visual query pattern to form a SPARQL query.
- Adding new nodes and edges.
- Interacting with the graph (zoom, move, rotate) 
- Using visual queries or a search function to add specific nodes

For more detailed instructions on how to use Graph2VR, please refer to the [Graph2VR User Manual](https://github.com/PjotrSvetachov/Graph2VR/blob/master/Graph2VR_User_manual.pdf).



In case you need a newer version of DotNetRDF if you develop the program further:
We had to adjust the DotNetRDF library and rebuild it, to make it work on the Quest2 headset.

Running the application without the small fix on the headset will give this error:

`Cannot use Type VDS.RDF.Parsing.GZippedNTriplesParser for the RDF Parser Type as it does not implement the required interface VDS.RDF.IRdfReader`

To fix this we made a custom version of the [library](https://github.com/dotnetrdf/dotnetrdf/blame/main/Libraries/dotNetRdf/Core/MimeTypeDefinition.cs#L398) that doesn't contain this check. We shiped it with Graph2VR.
