# Graph2VR

GraphVR is an application to Visualize Graphs (SPARQL) as 3D Graphs in Virtual Reality.
The idea is to explore, analyze, and interact with the data in the graph intuitively.

GraphVR has been built in Unity and is able to connect to a SPARQL endpoint using [dotNetRDF](https://dotnetrdf.org/).
We got inspired by many different tools to work with Linked data and to visualize Graphs.
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

## VDS.RDF fix to make the dotnetrdf library work on the Quest2 headset

Running the application on the headset will give this error
`Cannot use Type VDS.RDF.Parsing.GZippedNTriplesParser for the RDF Parser Type as it does not implement the required interface VDS.RDF.IRdfReader`
To fix this we made a custom version of the library that removes this check https://github.com/dotnetrdf/dotnetrdf/blame/main/Libraries/dotNetRdf/Core/MimeTypeDefinition.cs#L398 and shiped it with graph2vr.
