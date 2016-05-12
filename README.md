# Voxify
Voxify is an open-source 3D modeler for voxel based models. It will incorporate very basic features such as color & shader manipulation, as well as a dopesheet and curves sheet for simple animation.

Voxify is programmed in C#, using the SharpDX DirectX wrapper.

### FAQ

Q: "What platforms will the released product be available on?"
A: For now, Windows. I'm using DirectX which is a proprietary library for Windows. If I ever feel like ripping my eyes out of my sockets, I'll look into other options like OGRE for OpenGL. Hell, I'm kind of considering just doing it in Unity so I don't even have to worry about the OS constraint.

Q: "Why, Tristen, would you use a low level graphics library with a high level language?"
A: I'll beable to produce this software in a fashion that is less prone to error, and also in a language I am more comfortable with in an object oriented sense. When programming in C++, theres an unusual mixture of Data-Oriented and Object-Oriented design, which makes me uncomfortable. I also don't want to be too distracted by memory constraints, so I'd rather have the GC do most of the job for me. I probably would've done this in Jonathan Blow's Jai if the compiler was released.
