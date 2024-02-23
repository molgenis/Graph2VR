# Graph2VR

Graph2VR is a PhD project, a prototype for a VR application to visualize graphs (SPARQL) as 3D graphs in Virtual Reality.
The idea is to explore, analyze, and interact with the data in the graph using gesture control.

## System Requirements

### Hardware Requirements
- **VR Headset**: Graph2VR is designed for Virtual Reality headsets, with dedicated support for the HTC Vive and Oculus Quest series (Quest 2 and Quest 3). 
Compatibility with other VR headsets has not been verified. However, at least two controllers are required to control the app.


- **PC Specifications**: For the Windows version, a PC with the following minimum specifications is recommended:
  - **Processor**: Intel i5-4590 / AMD Ryzen 5 1500X or greater
  - **Memory**: 8GB+ RAM
  - **Graphics**: NVIDIA GTX 1060 / AMD Radeon RX 480 or greater
  - **Storage**: At least 4GB of free space is recommended.

### Software Requirements
- **Operating System**: Windows 10 or later for the PC version. The standalone version runs on Oculus Quest 2 and Quest 3 without a PC.

- **Unity Engine**: Users do not need Unity to run Graph2VR, but for development or modification, Unity version 2021.2.15f is advised.

### Additional Software
- **dotNetRDF**: Graph2VR uses the dotNetRDF library for connecting to SPARQL endpoints, bundled within the application.

### Standalone version Setup and Configuration
- Oculus Quest 2 and Quest 3 users must enable developer mode to install the standalone version via SideQuest.

## Installation Instructions

- **Windows Version**: Download the `Graph2VR_windows.zip` file from the [latest release](https://github.com/molgenis/Graph2VR/releases), unzip it, and execute the application.

- **Quest 2/3 Standalone Version**: Follow the instructions on SideQuest for sideloading the application onto your Oculus Quest headset.

The newest release can be found [here](https://github.com/molgenis/Graph2VR/releases).
It includes a Windows version (`Graph2VR_windows.zip`), and a standalone version for the Quest 2 or Quest 3 headset.

For a hands-on introduction to Graph2VR, we have created a video tutorial series that covers everything from basic navigation to advanced features. 
The tutorial is designed to help both beginners and experienced users get the most out of Graph2VR.

Check out the [Graph2VR Tutorial Series on YouTube](https://www.youtube.com/playlist?list=PLRQCsKSUyhNIdUzBNRTmE-_JmuiOEZbdH). 

Our tutorial is about using Graph2VR - it does not explain much about the basics of Linked Data and SPARQL.
If you want to learn more about the background of Linked Data, we can recommend the [Ted talk of Sir Tim Berners Lee "The next Web" (from 2009)](https://www.ted.com/talks/tim_berners_lee_the_next_web) for some basics.

Graph2VR has been built in Unity and is able to connect to a SPARQL endpoint using [dotNetRDF](https://dotnetrdf.org/).
We got inspired by many different tools to work with Linked data and to visualize Graphs.
Virtual Reality offers the user way more space to expand the graph than a 2D computer screen.

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

Our research paper about Graph2VR can be found at https://doi.org/10.1093/database/baae008 (soon).


