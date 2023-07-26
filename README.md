# Graph2VR

Graph2VR is a PhD project, a prototype for a VR application to Visualize Graphs (SPARQL) as 3D Graphs in Virtual Reality.
The idea is to explore, analyze, and interact with the data in the graph using gesture control.

Graph2VR has been built in Unity and is able to connect to a SPARQL endpoint using [dotNetRDF](https://dotnetrdf.org/).
We got inspired by many different tools to work with Linked data and to visualize Graphs.
Virtual Reality offers the user way more space to expand the graph, then a 2D computer screen.

Here is an example of a query that shows the results in form of stacked 2D graphs behind each other:

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

Different Layout Algorithms can help to visualize the data.

<img src="https://github.com/molgenis/Graph2VR/assets/49238704/673d2008-c93b-4e8f-9505-3cdcb2ba52cd" width="400">

For more detailed instructions on how to use Graph2VR, check out the [Graph2VR User Manual]( https://doi.org/10.5281/zenodo.8040594).

We recommend to load it on the Quest2 VR headset via [sidequest](https://sidequestvr.com/download)
