#! python 3

import random
import Rhino.Geometry as rg
import System
from System import Array
from math import sqrt
import clr
import traceback
import math

# Input variables (will be set by C# PythonScript.SetVariable)
# Default values for standalone execution
# if 'N0' not in globals():
#     N0 = 3
    
# if 'Steps' not in globals():
#     Steps = 300
    
# if 'dt' not in globals():
#     dt = 0.1
    
# if 'Lx' not in globals():
#     Lx = 10.0
    
# if 'Ly' not in globals():
#     Ly = 10.0
    
# if 'Lz' not in globals():
#     Lz = 10.0
    
# if 'speed' not in globals():
#     speed = 0.2
    
# if 'noise' not in globals():
#     noise = 0.05
    
# if 'repulsion_strength' not in globals():
#     repulsion_strength = 0.1
    
# if 'division_radius' not in globals():
#     division_radius = 0.1
    
# if 'division_rate' not in globals():
#     division_rate = 0.01

class Cell:
    def __init__(self, pos):
        self.pos = pos  # (x,y,z)
        self.history = [pos]  # trajectory tracking


def repulsion(cells, idx, strength, min_dist=0.1):
    cx, cy, cz = cells[idx].pos
    fx, fy, fz = 0.0, 0.0, 0.0

    for j in range(len(cells)):
        if j == idx: 
            continue

        x, y, z = cells[j].pos
        dx = cx - x
        dy = cy - y
        dz = cz - z

        dist = sqrt(dx*dx + dy*dy + dz*dz)
        if dist < min_dist and dist > 1e-6:
            factor = strength * (1.0/dist - 1.0/min_dist)
            fx += factor * (dx/dist)
            fy += factor * (dy/dist)
            fz += factor * (dz/dist)

    return fx, fy, fz


def simulate(
    N0=50,
    Steps=200,
    dt=0.1,
    Lx=10.0, Ly=10.0, Lz=5.0,
    speed=0.2,
    noise=0.05,
    repulsion_strength=0.1,
    division_rate=0.01,
    division_radius=0.1
):
    # Initialize cells
    cells = []
    for i in range(N0):
        x = random.random()*Lx
        y = random.random()*Ly
        z = random.random()*Lz
        cells.append(Cell((x,y,z)))

    # Simulation
    for step in range(Steps):
        new_cells = []

        for i in range(len(cells)):
            cx, cy, cz = cells[i].pos

            # random motility
            vx = (random.random()-0.5) * speed
            vy = (random.random()-0.5) * speed
            vz = (random.random()-0.5) * speed

            # Brownian noise
            vx += noise*(random.random()-0.5)
            vy += noise*(random.random()-0.5)
            vz += noise*(random.random()-0.5)

            # repulsion
            rx, ry, rz = repulsion(cells, i, repulsion_strength)
            vx += rx
            vy += ry
            vz += rz

            # update position
            nx = cx + vx*dt
            ny = cy + vy*dt
            nz = cz + vz*dt

            # reflect on boundaries
            nx = max(0, min(Lx, nx))
            ny = max(0, min(Ly, ny))
            nz = max(0, min(Lz, nz))

            cells[i].pos = (nx, ny, nz)
            cells[i].history.append((nx, ny, nz))

            # cell division
            if random.random() < division_rate:
                # new cell near parent
                bx = nx + division_radius*(random.random()-0.5)
                by = ny + division_radius*(random.random()-0.5)
                bz = nz + division_radius*(random.random()-0.5)

                bx = max(0, min(Lx, bx))
                by = max(0, min(Ly, by))
                bz = max(0, min(Lz, bz))

                new_cells.append(Cell((bx, by, bz)))

        # append newborns
        cells.extend(new_cells)

    return cells

# Initialize outputs
FinalPoints = []
Trajectories = []
Info = "Error: Simulation not executed"

cells = simulate(
    N0, Steps, dt,
    Lx, Ly, Lz,
    speed, noise,
    repulsion_strength,
    division_rate,
    division_radius
)

# Output A: Final points (rg.Point3d 리스트)
FinalPoints = [rg.Point3d(x, y, z) for (x, y, z) in (c.pos for c in cells)]

# Output B: Trajectories (rg.PolylineCurve 리스트)
Trajectories = []
for c in cells:
    pts = [rg.Point3d(px, py, pz) for (px, py, pz) in c.history]
    if len(pts) > 1:
        Trajectories.append(rg.PolylineCurve(pts))

# Output C: Info (그냥 문자열)
Info = "Initial: {0}\nFinal: {1}\nDivisions: {2}".format(N0, len(cells), len(cells) - N0)
