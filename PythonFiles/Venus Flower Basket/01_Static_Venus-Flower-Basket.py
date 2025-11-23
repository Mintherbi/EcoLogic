"""
Venus Flower Basket Structure Generator for Grasshopper
Creates Voronoi-like cellular pattern that can be applied to ANY surface
Perfect for facades, curved surfaces, or cylindrical forms

Inputs:
    surface: Surface - Target surface (if None, creates cylinder)
    height: float - Overall height (for default cylinder)
    diameter: float - Base diameter (for default cylinder)
    taper: float - Taper ratio (0-1, for default cylinder)
    fiber_diameter: float - Base thickness of fibers
    cell_density_u: int - Number of cells in U direction
    cell_density_v: int - Number of cells in V direction
    diagonal_guides: int - Number of diagonal guide lines
    include_framework: bool - Include diagonal lattice framework
    framework_thickness: float - Multiplier for framework thickness
    pattern_type: int - 0=Voronoi only, 1=Framework only, 2=Combined
    seed: int - Random seed for cell distribution
"""

import Rhino.Geometry as rg
import math
import random
import System

# Default values
if 'height' not in globals(): height = 100
if 'diameter' not in globals(): diameter = 150
if 'taper' not in globals(): taper = 0.1
if 'fiber_diameter' not in globals(): fiber_diameter = 2.5
if 'cell_density_u' not in globals(): cell_density_u = 30
if 'cell_density_v' not in globals(): cell_density_v = 40
if 'diagonal_guides' not in globals(): diagonal_guides = 15
if 'include_framework' not in globals(): include_framework = True
if 'framework_thickness' not in globals(): framework_thickness = 1.5
if 'pattern_type' not in globals(): pattern_type = 2
if 'seed' not in globals(): seed = 42

# Set random seed
random.seed(seed)

# Check if custom surface provided, otherwise create cylinder
if 'surface' not in globals() or surface is None:
    # Create default tapered cylinder
    circle = rg.Circle(rg.Plane.WorldXY, diameter/2)
    cylinder = rg.Cylinder(circle, height)
    base_surface = cylinder.ToBrep(True, True)
    if base_surface:
        target_surface = base_surface.Faces[0]
    print("Using default cylinder surface")
else:
    # Use provided surface - handle different input types
    import Rhino
    import scriptcontext as sc
    
    # If it's a GUID, get the actual geometry
    if isinstance(surface, System.Guid):
        obj = sc.doc.Objects.Find(surface)
        if obj:
            geometry = obj.Geometry
            if hasattr(geometry, 'Faces'):
                target_surface = geometry.Faces[0]
            else:
                target_surface = geometry
        else:
            print("ERROR: Could not find surface object")
            target_surface = None
    # If it's a Brep
    elif hasattr(surface, 'Faces'):
        target_surface = surface.Faces[0]
    # If it's already a surface
    else:
        target_surface = surface
    
    print("Using custom surface")

# Output lists
voronoi_fibers = []
framework_fibers = []
seed_points = []

def generate_voronoi_seeds_on_surface(surf, num_u, num_v):
    """Generate seed points distributed across surface UV space with variation"""
    seeds = []
    
    num_u = int(num_u)
    num_v = int(num_v)
    
    u_domain = surf.Domain(0)
    v_domain = surf.Domain(1)
    
    u_step = (u_domain.Max - u_domain.Min) / float(num_u - 1)
    v_step = (v_domain.Max - v_domain.Min) / float(num_v - 1)
    
    for i in range(num_u):
        for j in range(num_v):
            # Base UV coordinates
            u = u_domain.Min + i * u_step
            v = v_domain.Min + j * v_step
            
            # Add random variation
            u += random.uniform(-u_step * 0.3, u_step * 0.3)
            v += random.uniform(-v_step * 0.3, v_step * 0.3)
            
            # Clamp to domain
            u = max(u_domain.Min, min(u_domain.Max, u))
            v = max(v_domain.Min, min(v_domain.Max, v))
            
            # Get 3D point on surface
            point = surf.PointAt(u, v)
            seeds.append((u, v, point))
    
    return seeds

def find_voronoi_neighbors(seeds, surf, max_distance_factor=1.6):
    """Find neighboring Voronoi cells and create edges between them"""
    edges = []
    
    u_domain = surf.Domain(0)
    v_domain = surf.Domain(1)
    
    # Calculate average spacing
    if len(seeds) > 1:
        u_range = u_domain.Max - u_domain.Min
        v_range = v_domain.Max - v_domain.Min
        avg_u_spacing = u_range / math.sqrt(len(seeds))
        avg_v_spacing = v_range / math.sqrt(len(seeds))
    else:
        return edges
    
    for i, seed1 in enumerate(seeds):
        u1, v1, pt1 = seed1
        
        # Find nearby seeds
        for j, seed2 in enumerate(seeds):
            if i >= j:  # Avoid duplicates
                continue
            
            u2, v2, pt2 = seed2
            
            # Check UV distance first (faster)
            uv_dist = math.sqrt((u1-u2)**2 + (v1-v2)**2)
            
            # Only consider if close in UV space
            if uv_dist < max(avg_u_spacing, avg_v_spacing) * max_distance_factor:
                # Calculate 3D distance
                dist_3d = pt1.DistanceTo(pt2)
                
                # Connect if reasonable neighbors
                edges.append(((u1, v1, pt1), (u2, v2, pt2)))
    
    return edges

def create_curve_on_surface(surf, pt1_uv, pt2_uv, steps=8):
    """Create a curve between two points following the surface"""
    u1, v1 = pt1_uv[0], pt1_uv[1]
    u2, v2 = pt2_uv[0], pt2_uv[1]
    
    points = []
    for i in range(steps + 1):
        t = i / float(steps)
        u = u1 + (u2 - u1) * t
        v = v1 + (v2 - v1) * t
        point = surf.PointAt(u, v)
        points.append(point)
    
    if len(points) > 1:
        return rg.Curve.CreateInterpolatedCurve(points, 3)
    return None

# Generate pattern based on type
if pattern_type == 0 or pattern_type == 2:
    # Generate Voronoi seed points
    print("Generating Voronoi seeds...")
    seeds = generate_voronoi_seeds_on_surface(target_surface, cell_density_u, cell_density_v)
    seed_points = [s[2] for s in seeds]
    
    print("Creating Voronoi edges...")
    edges = find_voronoi_neighbors(seeds, target_surface)
    
    # Create curves and pipes for Voronoi cell edges
    print("Creating " + str(len(edges)) + " fiber geometries...")
    for seed1, seed2 in edges:
        curve = create_curve_on_surface(target_surface, seed1, seed2)
        
        if curve:
            pipe = rg.Brep.CreatePipe(curve, fiber_diameter/2, False, 
                                      rg.PipeCapMode.Round, True, 0.01, 0.01)
            if pipe:
                voronoi_fibers.extend(pipe)

# Add diagonal framework guides
if (pattern_type == 1 or pattern_type == 2) and include_framework:
    print("Creating diagonal framework...")
    
    diagonal_guides = int(diagonal_guides)
    u_domain = target_surface.Domain(0)
    v_domain = target_surface.Domain(1)
    
    num_steps = max(int(cell_density_v), 20)
    
    # Left diagonals (ascending)
    for diag in range(diagonal_guides):
        points = []
        
        for i in range(num_steps):
            t = i / float(num_steps - 1)
            
            # Move diagonally across UV space
            u = u_domain.Min + ((diag / float(diagonal_guides)) + t) % 1.0 * (u_domain.Max - u_domain.Min)
            v = v_domain.Min + t * (v_domain.Max - v_domain.Min)
            
            point = target_surface.PointAt(u, v)
            points.append(point)
        
        if len(points) > 1:
            curve = rg.Curve.CreateInterpolatedCurve(points, 3)
            if curve:
                pipe_radius = (fiber_diameter * framework_thickness) / 2
                pipe = rg.Brep.CreatePipe(curve, pipe_radius, False, 
                                          rg.PipeCapMode.Round, True, 0.01, 0.01)
                if pipe:
                    framework_fibers.extend(pipe)
    
    # Right diagonals (descending)
    for diag in range(diagonal_guides):
        points = []
        
        for i in range(num_steps):
            t = i / float(num_steps - 1)
            
            # Move diagonally in opposite direction
            u = u_domain.Min + ((diag / float(diagonal_guides)) - t) % 1.0 * (u_domain.Max - u_domain.Min)
            v = v_domain.Min + t * (v_domain.Max - v_domain.Min)
            
            point = target_surface.PointAt(u, v)
            points.append(point)
        
        if len(points) > 1:
            curve = rg.Curve.CreateInterpolatedCurve(points, 3)
            if curve:
                pipe_radius = (fiber_diameter * framework_thickness) / 2
                pipe = rg.Brep.CreatePipe(curve, pipe_radius, False, 
                                          rg.PipeCapMode.Round, True, 0.01, 0.01)
                if pipe:
                    framework_fibers.extend(pipe)

# Combine all geometry
all_fibers = voronoi_fibers + framework_fibers

# Output information
print("\nVenus Flower Basket Pattern Generated:")
print("  Pattern type: " + str(['Voronoi only', 'Framework only', 'Combined'][pattern_type]))
print("  Voronoi cell edges: " + str(len(voronoi_fibers)))
print("  Framework guides: " + str(len(framework_fibers)))
print("  Total elements: " + str(len(all_fibers)))
print("  Seed points: " + str(len(seed_points)))

# Assign outputs
a = all_fibers  # All combined geometry
b = voronoi_fibers  # Just Voronoi cells
c = framework_fibers  # Just diagonal framework
d = seed_points  # Seed points for visualization