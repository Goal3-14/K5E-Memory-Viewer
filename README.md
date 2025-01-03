# K5E

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)

K5E is a heap visualizer for Star Fox Adventures to help collect the 5th Krazoa Spirit early.

The code for this visualizer is ported from two projects:
1) [RenaKunisaki's script](https://github.com/RenaKunisaki/StarFoxAdventures/blob/master/misc-scripts/identifyPointer.py) - For the heap parsing logic
2) [Squalr](https://github.com/Squalr/Squalr/) - For the memory scanning libraries and window docking

Changes in K5E/Source/HeapVisualizer/HeapVisualizerViewModel.cs to allow other applications to access the heap values, frame count and other data. To be used with https://github.com/Goal3-14/SFA-Memory-Map and https://github.com/Goal3-14/SFA-Any-Practice.

![SqualrGUI](Documentation/K5E.png)

