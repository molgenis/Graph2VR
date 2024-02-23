# Graph2VR

Graph2VR is a PhD project, a prototype for a VR application to visualize graphs (SPARQL) as 3D graphs in Virtual Reality. The idea is to explore, analyze, and interact with the data in the graph using gesture control. Graph2VR has been built in Unity and is able to connect to a SPARQL endpoint using [dotNetRDF](https://dotnetrdf.org/). We got inspired by many different tools to work with Linked data and to visualize Graphs. Virtual Reality offers the user way more space to expand the graph than a 2D computer screen.

## Documentation

- For detailed instructions on how to use Graph2VR, refer to the [Graph2VR User Manual](https://doi.org/10.5281/zenodo.8040594).
- Our research paper on Graph2VR will be available soon [here](https://doi.org/10.1093/database/baae008).<br>
  The paper compares similar tools, provides insights into the design process of Graph2VR and contains the results from our usability study.

## Getting Started

For a hands-on introduction to Graph2VR, we have created a video tutorial series that covers everything from basic navigation to advanced features. 
The tutorial is designed to help both beginners and experienced users get the most out of Graph2VR.

Check out the [Graph2VR Tutorial Series on YouTube](https://www.youtube.com/playlist?list=PLRQCsKSUyhNIdUzBNRTmE-_JmuiOEZbdH). 

For those new to Linked Data and SPARQL, we recommend Sir Tim Berners Lee's [Ted talk "The next Web"](https://www.ted.com/talks/tim_berners_lee_the_next_web) for some basics.

## Installation Instructions

The newest release can be found [here](https://github.com/molgenis/Graph2VR/releases).
It includes a Windows version (`Graph2VR_windows.zip`), and a standalone version for the Quest 2 or Quest 3 headset.

For the **Windows Version**: 
- Download the `Graph2VR_windows.zip` file from the [latest release](https://github.com/molgenis/Graph2VR/releases), unzip it, and execute the application.

For the **Quest 2/3 Standalone Version**:
- Ensure your Oculus Quest 2 or Quest 3 is in developer mode to install the standalone version via SideQuest.
- We recommend [SideQuest](https://sidequestvr.com/download) for loading the application onto the Quest2/3 VR headset.

## Features

Here is an example of a query that shows the results in form of stacked 2D graphs behind each other:

<img src="https://github.com/molgenis/Graph2VR/assets/49238704/aa144a7e-96c6-474b-b8b4-a807d1b3e6b1" width="400">

We tried to write a GUI to explore and interact with the graph in Virtual Reality. 
A few simple operations to do so are:

- Getting information about the current node
- Expanding the graph at a node using incoming or outgoing predicates
- Deleting a node or collapsing unconnected nodes
- Comparing different parts of the graph side by side
- Building visual query patterns for SPARQL queries
- Adding new nodes and edges
- Interacting with the graph (zoom, move, rotate)
- Using visual queries or a search function to add specific nodes
- Saving results in ntriple format for reuse in other programs

<img src="https://github.com/molgenis/Graph2VR/assets/49238704/45a87902-f7f3-43d7-8e38-d05b2a12bb35" width="400">

Different Layout Algorithms can help to visualize the data.

<img src="https://github.com/molgenis/Graph2VR/assets/49238704/673d2008-c93b-4e8f-9505-3cdcb2ba52cd" width="400">

## System Requirements

### Hardware Requirements
- **VR Headset**: Graph2VR is designed for Virtual Reality headsets, with dedicated support for the HTC Vive and Oculus Quest series (Quest 2 and Quest 3). 
Compatibility with other VR headsets has not been verified. However, at least two controllers are required to control the app.


### PC Specifications
  - **Processor**: Intel i5-4590 / AMD Ryzen 5 1500X or greater
  - **Memory**: 8GB+ RAM
  - **Graphics**: NVIDIA GTX 1060 / AMD Radeon RX 480 or greater
  - **Storage**: At least 4GB of free space is recommended.

### Software Requirements
- **Operating System**: Windows 10 or later for PC version; standalone version available for Oculus Quest 2 and Quest 3.
- **Unity Engine**: For development, Unity version 2021.2.15f is recommended.
