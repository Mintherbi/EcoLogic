#! python 3

import random
import Rhino.Geometry as rg
import System
from System import Array
from math import sqrt
import clr
import traceback
import math

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

def main(N0, Steps, dt, Lx, Ly, Lz, speed, noise,
         repulsion_strength, division_radius, division_rate):

    cells = simulate(N0, Steps, dt, Lx, Ly, Lz, speed, noise,
                     repulsion_strength, division_rate, division_radius)

    final_points = [list(c.pos) for c in cells]

    trajectories = []
    for c in cells:
        trajectories.append([list(p) for p in c.history])

    info = f"Initial: {N0}, Final: {len(cells)}, Divisions: {len(cells)-N0}"

    return final_points, trajectories, info

final_points, trajectories, info = main(
    N0, Steps, dt, Lx, Ly, Lz,
    speed, noise, repulsion_strength,
    division_radius, division_rate
)

Outputs['FinalPoints'] = final_points
Outputs['Trajectories'] = trajectories
Outputs['Info'] = info
