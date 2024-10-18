Hello! 

To run my building generation code, you can simply start by selecting a seed and clicking play. 
Then, you can use the arrow keys to move around the scene, or you can click on scene view to get a nice view from above of the buildings.
A ground plane was placed to allow the illusion of walking around the setting, but all buildings have floor geometry as well. This is viewable in Scene view and by tilting the camera.
It is guarenteed that you will get 3 unique floor plans.

As you move in the positive z direction, you will see the other buildings that are generated.

The building style was residential - specifically, apartment complexes, hotels, or motels were my inspiration. 
Basically, two building types are generated: a brick and a stone one. 
There are two types of windows: a blue square one with a gradient, and a diamond one with a funky checkerboard pattern.
There are two types of doors: a single door that was inspired by Minecraft, and a wooden, striped double door.

All colorings are done with procedurally generated textures, and random numbers.
Specficially, the stone texture uses squared random numbers to shift the pdf.
And some are fun shapes, like "S + D" or a heart shape to show off the power of this generating code!

You can also create your own floor plan! using the Custom Map variable in the inspector, write 0 for no building and any non zero integer for building at that spot.
You must select the Use Custom Map checkbox and keep the Custom Map size to 25 for this to work. 

Here are some notable seeds (should show off most of my floor plans): 
19394
12390847 
2147483647
123123433

Thank you and I hope you enjoy!