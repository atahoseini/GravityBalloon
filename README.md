# Gravity Planet

## Program Overview

This program allows users to interact with circles displayed on a canvas. It features the following functionalities:

- **Input:** The program provides three text boxes where users can input coordinates for circles. There are buttons for adding, animating, and clearing the canvas.

- **Display:** All input coordinates are presented in a data grid. The canvas is used to visually display the circles.

- **Planet Class:** A `Planet` class is defined to represent the circles. A list of instances of this class is created to manage the circles.

- **Adding Circles:** By clicking the 'ADD' button, user-entered coordinates are read and a new `Planet` instance is created. Correct positioning is ensured, and overlaps are avoided.

- **Overlap Handling:** The `SetPosition` function checks for overlaps and adjusts the position of the new shape using the `AdjustPosition` function. Overlaps are determined using the `CheckOverlap` function, which returns `TRUE` or `FALSE` based on minimum distance considerations.

- **Drawing:** Circles are drawn using the `DrawItem` function, which returns an `Ellipse`. The information is stored in a dictionary that associates the `Ellipse` with the circle's coordinates.

- **Animation:** A timer with a 50-millisecond interval is used for animation. In each timer tick, a double-loop (foreach) checks for gravitational interactions between circles. Gravitational forces and angles of attraction are calculated using the `CalculateForce` function.

- **Updating Position:** After gravitational effects are calculated for all circles, their new positions are updated and reflected on the canvas using the `Canvas.SetLeft` and `Canvas.SetTop` functions.

- **Mouse Interaction:** The program also includes a simplified mouse drawing feature below the DataGrid, demonstrating mouse click events.

## Usage

To use this program, follow these steps:

1. Input the coordinates of circles using the provided text boxes.
2. Click the 'ADD' button to add the circles to the canvas.
3. Click the 'Play' button to start the animation.
4. Use the 'Clear' button to reset the canvas and remove all circles.


![2023-08-31_11-13-34](https://github.com/atahoseini/GravityBalloon/assets/9142175/7694a660-0f67-445b-8f16-e25b03617863)
