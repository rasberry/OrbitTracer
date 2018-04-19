# Orbit Tracer

This program traces [Mandelrot](https://en.wikipedia.org/wiki/Mandelbrot_set) fractal orbits.
<br/>There are two modes

## UI mode
With this mode you can trace orbits by clicking on the pixels within the window.
<br/>Actions:
* Clicking (or holding down) the left mouse button draws that points orbit in red
* Dragging with the middle mouse button moves the rendered image
```
>OrbitTracer.exe
```

## CLI mode
With this mode you can render the fractal image to a PNG file
```
>OrbitTracer.exe -h

OrbitTracer [options] (filename / prefix)
Options:
 --help / -h                       Show this help
 -d (width) (height)               Size of image output images in pixels
 -r (resolution)                   Scale factor (Default: 200. 400 = 2x bigger)
 -o                                Output orbits instead of nebulabrot
                                    (Warning: produces one image per coordinate)
```