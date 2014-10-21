#ubonsai (Terminated)

A simple C# [behaviour tree](http://www.altdevblogaday.com/2011/02/24/introduction-to-behavior-trees/) library with a node based custom editor for the Unity game engine, all licensed under the MIT license.

##Motivation

There are existing behaviour tree solutions for Unity, some are paid, others are free but closed source. None tick all the right boxes for me though, so instead of working on my damn game I'm getting side-tracked into writing my own solution. I don't plan on doing any fancy optimizations, I just need it to work.

##Roadmap

- Build a custom editor to create behaviour trees in Unity. (Started)
- Write a library to load and run the behaviour trees created via the custom editor.
- Add real-time execution tracing and breakpoints to the custom editor to allow for easy debugging.

##Building

Not much too it at the moment:
- Copy the source from **src/editor** to **YourProject/Assets/UBonsai/Editor**.
- Give Unity a few seconds to recompile.
- Open the **UBonsai Editor** window from the **Window** sub-menu in the Unity editor.
