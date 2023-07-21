# Graph2VR

Graph2VR is a PhD project, a prototype for a VR application to Visualize Graphs (SPARQL) as 3D Graphs in Virtual Reality.
The idea is to explore, analyze, and interact with the data in the graph using gesture control.

Graph2VR has been built in Unity and is able to connect to a SPARQL endpoint using [dotNetRDF](https://dotnetrdf.org/).
We got inspired by many different tools to work with Linked data and to visualize Graphs.
Virtual Reality offers the user way more space to expand the graph, then a 2D computer screen.

<img src="https://github.com/molgenis/Graph2VR/assets/49238704/aa144a7e-96c6-474b-b8b4-a807d1b3e6b1" width="400">

We tried to write a GUI to explore and interact with the graph in Virtual Reality. 
A few simple operations to do so are:

<ul>
  <li>Getting some information about the current node</li>
  <li>Expanding the graph at a certain node using incoming or outgoing predicates</li>
  <li>Deleting a node or collapsing those nodes around it that are not connected to the graph elsewhere</li>
  <li>Comparing different parts of the graph side by side (different visualisations)</li>
  <li>Building visual query patterns to form a SPARQL query</li>
  <li>Adding new nodes and edges</li>
  <li>Interacting with the graph (zoom, move, rotate)</li>
  <li>Using visual queries or a search function to add specific nodes</li>
  <li>Saving the results to ntriple format to be able to reuse it in other programs</li>
</ul>

<img src="https://github.com/molgenis/Graph2VR/assets/49238704/45a87902-f7f3-43d7-8e38-d05b2a12bb35" width="400">

<img src="https://github.com/molgenis/Graph2VR/assets/49238704/673d2008-c93b-4e8f-9505-3cdcb2ba52cd" width="400">


For more detailed instructions on how to use Graph2VR, please refer to the [Graph2VR User Manual]( https://doi.org/10.5281/zenodo.8040594).


<p>
In case you need a newer version of DotNetRDF if you develop the program further:
We had to adjust the DotNetRDF library and rebuild it, to make it work on the Quest2 headset.
Running the application without the small fix on the headset will give this error:

`Cannot use Type VDS.RDF.Parsing.GZippedNTriplesParser for the RDF Parser Type as it does not implement the required interface VDS.RDF.IRdfReader`

To fix this we made a custom version of the [library](https://github.com/dotnetrdf/dotnetrdf/blame/main/Libraries/dotNetRdf/Core/MimeTypeDefinition.cs#L398) that doesn't contain this check. We shipped it with Graph2VR.
</p>
