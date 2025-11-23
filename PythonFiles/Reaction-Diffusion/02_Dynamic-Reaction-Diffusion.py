import Rhino.Geometry as rg
import random
import scriptcontext as sc
from math import sqrt
from Grasshopper import DataTree
from Grasshopper.Kernel.Data import GH_Path

class Cell:
    def __init__(self, pos):
        self.pos = pos
        self.history = [pos]

def repulsion(cells, idx, strength, min_dist=0.1):
    cx, cy, cz = cells[idx].pos
    fx = fy = fz = 0.0

    for j in range(len(cells)):
        if j == idx:
            continue

        x, y, z = cells[j].pos
        dx = cx - x
        dy = cy - y
        dz = cz - z

        d = sqrt(dx*dx + dy*dy + dz*dz)
        if d < min_dist and d > 1e-6:
            factor = strength * (1.0/d - 1.0/min_dist)
            fx += factor * (dx/d)
            fy += factor * (dy/d)
            fz += factor * (dz/d)

    return fx, fy, fz


# ---------------------------------------------------------
# SINGLE STEP UPDATE
# ---------------------------------------------------------
def update_cells(cells, dt, Lx, Ly, Lz, speed, noise, repulsion_strength, div_rate, div_radius):
    new_cells = []

    for i in range(len(cells)):
        cx, cy, cz = cells[i].pos

        # Movement (Random walk + noise)
        vx = (random.random()-0.5) * speed
        vy = (random.random()-0.5) * speed
        vz = (random.random()-0.5) * speed

        vx += noise*(random.random()-0.5)
        vy += noise*(random.random()-0.5)
        vz += noise*(random.random()-0.5)

        # Repulsion
        rx, ry, rz = repulsion(cells, i, repulsion_strength)
        vx += rx
        vy += ry
        vz += rz

        # Update position
        nx = max(0, min(Lx, cx + vx*dt))
        ny = max(0, min(Ly, cy + vy*dt))
        nz = max(0, min(Lz, cz + vz*dt))

        cells[i].pos = (nx, ny, nz)
        cells[i].history.append((nx, ny, nz))

        # Division
        if random.random() < div_rate:
            bx = max(0, min(Lx, nx + div_radius*(random.random()-0.5)))
            by = max(0, min(Ly, ny + div_radius*(random.random()-0.5)))
            bz = max(0, min(Lz, nz + div_radius*(random.random()-0.5)))

            new_cells.append(Cell((bx,by,bz)))

    cells.extend(new_cells)
    return cells, len(new_cells)


# ---------------------------------------------------------
# MAIN GH EXECUTION
# ---------------------------------------------------------

# Sticky keys
KEY_CELLS = "cells_state"
KEY_STEP  = "step_state"

# Reset simulation
if Reset or (KEY_CELLS not in sc.sticky):
    cells = []
    for i in range(N0):
        cells.append(Cell((
            random.random()*Lx,
            random.random()*Ly,
            random.random()*Lz
        )))
    sc.sticky[KEY_CELLS] = cells
    sc.sticky[KEY_STEP] = 0

cells = sc.sticky[KEY_CELLS]
step  = sc.sticky[KEY_STEP]


if Run:
    cells, new_count = update_cells(
        cells,
        dt,
        Lx, Ly, Lz,
        speed,
        noise,
        repulsion_strength,
        division_rate,
        division_radius
    )
    step += 1

    sc.sticky[KEY_CELLS] = cells
    sc.sticky[KEY_STEP] = step
else:
    new_count = 0


# ---------------------------------------------------------
# OUTPUT: Points + Trajectories + Info
# ---------------------------------------------------------

# --- Points as separate branches ---
PointsTree = DataTree[rg.Point3d]()
for i, c in enumerate(cells):
    p = rg.Point3d(*c.pos)
    PointsTree.Add(p, GH_Path(i))


# --- Trajectories as separate branches ---
TrajTree = DataTree[rg.Curve]()
for i, c in enumerate(cells):
    if len(c.history) > 1:
        pts = [rg.Point3d(*p) for p in c.history]
        curve = rg.PolylineCurve(pts)
        TrajTree.Add(curve, GH_Path(i))


# Debug Info
Info = "Step: {}\nCells: {}\n".format(step, len(cells))
