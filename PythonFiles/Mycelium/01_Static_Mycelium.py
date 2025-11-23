from Rhino.Geometry import Point3d, Line, RTree, BoundingBox
import math
import random

# ---------- robust point reader ----------
def collect_points(data):
    """Recursively collect any Point3d-like things from GH input into a flat list."""
    pts = []

    def add_item(item):
        if isinstance(item, Point3d):
            pts.append(item)
            return
        if hasattr(item, "X") and hasattr(item, "Y") and hasattr(item, "Z"):
            pts.append(Point3d(item.X, item.Y, item.Z))
            return
        if hasattr(item, "Location"):
            p = item.Location
            pts.append(Point3d(p.X, p.Y, p.Z))
            return
        if hasattr(item, "Value"):
            add_item(item.Value)
            return
        try:
            for sub in item:
                add_item(sub)
        except TypeError:
            pass

    add_item(data)
    return pts

# ---------- RTree helpers ----------
def build_rtree(nodes):
    tree = RTree()
    for i, n in enumerate(nodes):
        tree.Insert(n.pos, i)
    return tree

def neighbors(tree, pt, radius):
    """Return indices of nodes within radius of pt using bounding box search."""
    ids = []

    def cb(sender, e):
        ids.append(e.Id)

    r = radius
    bb = BoundingBox(
        Point3d(pt.X - r, pt.Y - r, pt.Z - r),
        Point3d(pt.X + r, pt.Y + r, pt.Z + r)
    )
    tree.Search(bb, cb, None)
    return ids

# ---------- read inputs ----------
start_pts  = collect_points(StartPts)   # seeds (you probably have 1 in the middle)
all_food   = collect_points(FoodPts)    # all potential nutrients in 3D

Segments = []

if not start_pts:
    Segments = []
    Tips = []
else:
    Step    = float(Step)
    SenseR  = float(SenseR)
    KillR   = float(KillR)
    MaxIter = int(max(0, MaxIter))
    GrowR   = float(GrowR)

    # ---------- filter nutrients by growth radius (3D sphere) ----------
    attractors = []
    if GrowR > 0:
        growR2 = GrowR * GrowR
        for a in all_food:
            for s in start_pts:
                if s.DistanceToSquared(a) <= growR2:
                    attractors.append(a)
                    break

    if not attractors:
        Segments = []
        Tips = start_pts
    else:
        class Node(object):
            def __init__(self, pos, parent_index=-1):
                self.pos = Point3d(pos)
                self.parent = parent_index

        nodes = [Node(p, -1) for p in start_pts]

        # ---------- main growth loop ----------
        for it in range(MaxIter):
            if not attractors:
                break

            node_tree = build_rtree(nodes)

            influences = [[] for _ in nodes]
            to_remove  = set()

            SenseR2 = SenseR * SenseR
            KillR2  = KillR * KillR

            # 1) assign attractors to nearby nodes via RTree
            for ai, a in enumerate(attractors):
                near_ids = neighbors(node_tree, a, SenseR)
                if not near_ids:
                    continue

                nearest_i = None
                nearest_d2 = None

                for ni in near_ids:
                    n = nodes[ni]
                    d2 = n.pos.DistanceToSquared(a)

                    # eat food if very close
                    if d2 <= KillR2:
                        to_remove.add(ai)
                        nearest_i = None
                        break

                    if d2 <= SenseR2 and (nearest_d2 is None or d2 < nearest_d2):
                        nearest_d2 = d2
                        nearest_i = ni

                if nearest_i is not None:
                    influences[nearest_i].append(a - nodes[nearest_i].pos)

            # 2) remove consumed attractors
            if to_remove:
                attractors = [a for idx, a in enumerate(attractors) if idx not in to_remove]
                if not attractors:
                    break

            # 3) grow new nodes from influenced nodes (3D)
            new_nodes = []
            for ni, n in enumerate(nodes):
                dirs = influences[ni]
                if not dirs:
                    continue

                # average influence
                vx = vy = vz = 0.0
                for v in dirs:
                    vx += v.X
                    vy += v.Y
                    vz += v.Z

                length = math.sqrt(vx*vx + vy*vy + vz*vz)
                if length == 0.0:
                    continue

                vx /= length
                vy /= length
                vz /= length

                # --- 3D jitter for organic branching ---
                j = 0.2  # try 0.1–0.3
                vx += random.uniform(-j, j)
                vy += random.uniform(-j, j)
                vz += random.uniform(-j, j)

                length2 = math.sqrt(vx*vx + vy*vy + vz*vz)
                if length2 == 0.0:
                    continue

                vx /= length2
                vy /= length2
                vz /= length2

                new_pos = Point3d(
                    n.pos.X + vx * Step,
                    n.pos.Y + vy * Step,
                    n.pos.Z + vz * Step
                )

                child_index = len(nodes) + len(new_nodes)
                new_nodes.append(Node(new_pos, ni))
                Segments.append(Line(n.pos, new_pos))

            if not new_nodes:
                break

            nodes.extend(new_nodes)

        # outputs
        Segments = [ln.ToNurbsCurve() for ln in Segments]
        parent_ids = set(n.parent for n in nodes if n.parent >= 0)
        Tips = [n.pos for i, n in enumerate(nodes) if i not in parent_ids]