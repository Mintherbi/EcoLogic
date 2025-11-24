"""
TRABECULAR STRUCTURE PROTOTYPE - REFINED VERSION
More porous, thinner members, irregular floors
"""

import Rhino.Geometry as rg
import math
import random
from collections import defaultdict

class TrabeculaGenerator:
    
    def __init__(self, boundary, floor_count, floor_height, voxel_size):
        self.boundary = boundary
        self.floor_count = floor_count
        self.floor_height = floor_height
        self.voxel_size = voxel_size
        
        self.bbox = boundary.GetBoundingBox(True)
        self.voxels = {}
        self.stress_field = {}
        self.load_points = []
        
    def generate_voxel_grid(self):
        """Create 3D voxel grid within boundary"""
        print("Generating voxel grid...")
        
        voxels = []
        x_min, y_min, z_min = self.bbox.Min.X, self.bbox.Min.Y, self.bbox.Min.Z
        x_max, y_max, z_max = self.bbox.Max.X, self.bbox.Max.Y, self.bbox.Max.Z
        
        x = x_min
        while x <= x_max:
            y = y_min
            while y <= y_max:
                z = z_min
                while z <= z_max:
                    pt = rg.Point3d(x, y, z)
                    
                    if self.boundary.IsPointInside(pt, 0.01, True):
                        voxel_key = self._get_voxel_key(pt)
                        self.voxels[voxel_key] = {
                            'center': pt,
                            'stress': 0.0,
                            'density': 0.0,
                            'is_floor': False,
                            'is_column': False
                        }
                    
                    z += self.voxel_size
                y += self.voxel_size
            x += self.voxel_size
        
        print("Created {} voxels".format(len(self.voxels)))
        return self.voxels
    
    def _get_voxel_key(self, point):
        """Convert point to discrete voxel coordinate"""
        x = int(round(point.X / self.voxel_size))
        y = int(round(point.Y / self.voxel_size))
        z = int(round(point.Z / self.voxel_size))
        return (x, y, z)
    
    def _key_to_point(self, key):
        """Convert voxel key back to point"""
        x = key[0] * self.voxel_size
        y = key[1] * self.voxel_size
        z = key[2] * self.voxel_size
        return rg.Point3d(x, y, z)
    
    def define_load_points(self):
        """Generate load points at each floor level"""
        print("Defining load points...")
        
        self.load_points = []
        
        for floor in range(self.floor_count):
            z_height = self.bbox.Min.Z + (floor + 1) * self.floor_height
            
            x_samples = int((self.bbox.Max.X - self.bbox.Min.X) / (self.voxel_size * 3)) + 1
            y_samples = int((self.bbox.Max.Y - self.bbox.Min.Y) / (self.voxel_size * 3)) + 1
            
            for i in range(x_samples):
                for j in range(y_samples):
                    x = self.bbox.Min.X + i * (self.bbox.Max.X - self.bbox.Min.X) / max(1, x_samples - 1)
                    y = self.bbox.Min.Y + j * (self.bbox.Max.Y - self.bbox.Min.Y) / max(1, y_samples - 1)
                    
                    load_pt = rg.Point3d(x, y, z_height)
                    
                    if self.boundary.IsPointInside(load_pt, 0.01, True):
                        load_magnitude = 5.0
                        self.load_points.append({
                            'point': load_pt,
                            'magnitude': load_magnitude,
                            'floor': floor
                        })
        
        print("Created {} load points".format(len(self.load_points)))
        return self.load_points
    
    def calculate_stress_field(self, wind_vector=None):
        """Calculate stress field using distance-based load diffusion"""
        print("Calculating stress field...")
        
        for key in self.voxels:
            self.voxels[key]['stress'] = 0.0
        
        ground_z = self.bbox.Min.Z
        
        for load in self.load_points:
            load_pt = load['point']
            load_mag = load['magnitude']
            
            for voxel_key, voxel_data in self.voxels.items():
                voxel_pt = voxel_data['center']
                
                if voxel_pt.Z <= load_pt.Z:
                    distance = voxel_pt.DistanceTo(load_pt)
                    vertical_dist = abs(load_pt.Z - voxel_pt.Z)
                    horizontal_dist = math.sqrt(
                        (load_pt.X - voxel_pt.X)**2 + 
                        (load_pt.Y - voxel_pt.Y)**2
                    )
                    
                    if distance > 0:
                        vertical_factor = math.exp(-horizontal_dist / (self.voxel_size * 5))
                        stress_contribution = (load_mag / (vertical_dist + 1.0)) * vertical_factor
                        ground_factor = 1.0 + (1.0 - (voxel_pt.Z - ground_z) / (self.bbox.Max.Z - ground_z)) * 0.5
                        voxel_data['stress'] += stress_contribution * ground_factor
        
        if wind_vector and wind_vector.Length > 0:
            self._add_wind_stress(wind_vector)
        
        max_stress = max([v['stress'] for v in self.voxels.values()] + [0.001])
        for voxel_data in self.voxels.values():
            voxel_data['stress'] /= max_stress
        
        print("Stress field calculated")
    
    def _add_wind_stress(self, wind_vector):
        """Add lateral wind loads to stress field"""
        wind_dir = rg.Vector3d(wind_vector)
        wind_dir.Unitize()
        
        for voxel_key, voxel_data in self.voxels.items():
            voxel_pt = voxel_data['center']
            height_ratio = (voxel_pt.Z - self.bbox.Min.Z) / (self.bbox.Max.Z - self.bbox.Min.Z)
            exposure = max(0, wind_dir.X * (voxel_pt.X - self.bbox.Min.X) + 
                              wind_dir.Y * (voxel_pt.Y - self.bbox.Min.Y))
            wind_stress = height_ratio * exposure * 0.3
            voxel_data['stress'] += wind_stress
    
    def generate_density_map(self, threshold=0.1):
        """Convert stress field to material density"""
        print("Generating density map...")
        
        for voxel_key, voxel_data in self.voxels.items():
            stress = voxel_data['stress']
            
            if stress > threshold:
                # More aggressive power law for sparser result
                density = stress ** 1.2  # Changed from 0.7 to 1.2
                voxel_data['density'] = min(1.0, density)
            else:
                voxel_data['density'] = 0.0
        
        print("Density map generated")
    
    def identify_floors_and_columns(self):
        """Identify which voxels should be floors or columns"""
        print("Identifying architectural elements...")
        
        floor_tolerance = self.voxel_size * 0.6
        
        for floor_idx in range(self.floor_count):
            target_z = self.bbox.Min.Z + (floor_idx + 1) * self.floor_height
            
            for voxel_key, voxel_data in self.voxels.items():
                voxel_pt = voxel_data['center']
                
                # Lower threshold for floor identification
                if abs(voxel_pt.Z - target_z) < floor_tolerance and voxel_data['density'] > 0.05:
                    voxel_data['is_floor'] = True
        
        self._identify_columns()
        print("Architectural elements identified")
    
    def _identify_columns(self):
        """Identify vertical load paths as columns"""
        xy_columns = defaultdict(list)
        
        for voxel_key, voxel_data in self.voxels.items():
            if voxel_data['density'] > 0.4:  # Higher threshold for columns
                xy_key = (voxel_key[0], voxel_key[1])
                xy_columns[xy_key].append((voxel_key[2], voxel_key))
        
        for xy_key, z_list in xy_columns.items():
            z_list.sort()
            
            if len(z_list) >= self.floor_count * 0.5:
                for z_coord, voxel_key in z_list:
                    if self.voxels[voxel_key]['stress'] > 0.5:  # Higher threshold
                        self.voxels[voxel_key]['is_column'] = True
    
    def generate_lattice(self):
        """Create structural lattice connecting high-density voxels"""
        print("Generating trabecular lattice...")
        
        lattice_lines = []
        
        for voxel_key, voxel_data in self.voxels.items():
            # Higher minimum density threshold
            if voxel_data['density'] < 0.3:  # Changed from 0.2 to 0.3
                continue
            
            center = voxel_data['center']
            
            for dx in [-1, 0, 1]:
                for dy in [-1, 0, 1]:
                    for dz in [-1, 0, 1]:
                        if dx == 0 and dy == 0 and dz == 0:
                            continue
                        
                        neighbor_key = (voxel_key[0] + dx, 
                                      voxel_key[1] + dy, 
                                      voxel_key[2] + dz)
                        
                        if neighbor_key in self.voxels:
                            neighbor_data = self.voxels[neighbor_key]
                            
                            # Higher threshold for connections
                            if neighbor_data['density'] >= 0.3:  # Changed from 0.2
                                neighbor_center = neighbor_data['center']
                                avg_density = (voxel_data['density'] + neighbor_data['density']) / 2.0
                                line = rg.Line(center, neighbor_center)
                                
                                if self._should_add_line(line, avg_density):
                                    lattice_lines.append({
                                        'line': line.ToNurbsCurve(),
                                        'density': avg_density,
                                        'is_column': voxel_data['is_column'] or neighbor_data['is_column']
                                    })
        
        print("Generated {} lattice members".format(len(lattice_lines)))
        return lattice_lines
    
    def _should_add_line(self, line, density):
        """Stochastic filtering - MORE AGGRESSIVE"""
        # Higher power means fewer weak connections
        probability = density ** 3  # Changed from 2 to 3
        return random.random() < probability
    
    def generate_floor_slabs(self):
        """Create IRREGULAR floor slab surfaces from floor-level voxels"""
        print("Generating irregular floor slabs...")
        
        floor_slabs = []
        
        for floor_idx in range(self.floor_count):
            floor_points = []
            
            for voxel_key, voxel_data in self.voxels.items():
                if voxel_data['is_floor']:
                    target_z = self.bbox.Min.Z + (floor_idx + 1) * self.floor_height
                    
                    if abs(voxel_data['center'].Z - target_z) < self.voxel_size:
                        floor_points.append(voxel_data['center'])
            
            if len(floor_points) > 3:
                target_z = self.bbox.Min.Z + (floor_idx + 1) * self.floor_height
                floor_points_2d = [rg.Point3d(pt.X, pt.Y, target_z) for pt in floor_points]
                
                if len(floor_points_2d) >= 4:
                    # Use actual convex hull for irregular shape
                    perimeter_pts = self._get_convex_hull(floor_points_2d)
                    
                    if len(perimeter_pts) >= 3:
                        perimeter_pts.append(perimeter_pts[0])  # Close curve
                        floor_curve = rg.Curve.CreateControlPointCurve(perimeter_pts, 1)
                        
                        if floor_curve and floor_curve.IsClosed:
                            floor_brep = rg.Brep.CreatePlanarBreps(floor_curve, 0.01)
                            if floor_brep:
                                floor_slabs.append({
                                    'brep': floor_brep[0],
                                    'level': floor_idx,
                                    'height': target_z
                                })
        
        print("Generated {} irregular floor slabs".format(len(floor_slabs)))
        return floor_slabs
    
    def _get_convex_hull(self, points):
        """
        Get actual convex hull for irregular floor boundary
        Graham scan algorithm
        """
        if len(points) < 3:
            return points
        
        # Find the point with lowest Y (and leftmost if tie)
        start = min(points, key=lambda p: (p.Y, p.X))
        
        # Sort points by polar angle with respect to start point
        def polar_angle(p):
            dx = p.X - start.X
            dy = p.Y - start.Y
            return math.atan2(dy, dx)
        
        sorted_points = sorted([p for p in points if p != start], key=polar_angle)
        
        # Build convex hull
        hull = [start]
        
        for p in sorted_points:
            # Remove points that make a right turn
            while len(hull) > 1:
                # Cross product to determine turn direction
                v1_x = hull[-1].X - hull[-2].X
                v1_y = hull[-1].Y - hull[-2].Y
                v2_x = p.X - hull[-1].X
                v2_y = p.Y - hull[-1].Y
                cross = v1_x * v2_y - v1_y * v2_x
                
                if cross > 0:  # Left turn - keep going
                    break
                else:  # Right turn - remove last point
                    hull.pop()
            
            hull.append(p)
        
        return hull
    
    def _get_perimeter_points(self, points):
        """Fallback: simplified perimeter"""
        if len(points) < 3:
            return points
        
        center = rg.Point3d(
            sum(pt.X for pt in points) / len(points),
            sum(pt.Y for pt in points) / len(points),
            points[0].Z
        )
        
        def angle_from_center(pt):
            return math.atan2(pt.Y - center.Y, pt.X - center.X)
        
        sorted_points = sorted(points, key=angle_from_center)
        step = max(1, len(sorted_points) // 12)
        return sorted_points[::step]
    
    def extract_columns(self):
        """Extract primary column lines from lattice"""
        print("Extracting columns...")
        
        columns = []
        xy_groups = defaultdict(list)
        
        for voxel_key, voxel_data in self.voxels.items():
            if voxel_data['is_column']:
                xy_key = (voxel_key[0], voxel_key[1])
                xy_groups[xy_key].append(voxel_data['center'])
        
        for xy_key, points in xy_groups.items():
            if len(points) >= 2:
                points.sort(key=lambda p: p.Z)
                column_line = rg.Line(points[0], points[-1])
                columns.append({
                    'line': column_line.ToNurbsCurve(),
                    'base': points[0],
                    'top': points[-1],
                    'height': points[-1].Z - points[0].Z
                })
        
        print("Extracted {} primary columns".format(len(columns)))
        return columns
    
    def get_stress_visualization(self):
        """Create colored points for stress visualization"""
        stress_viz = []
        
        for voxel_key, voxel_data in self.voxels.items():
            if voxel_data['density'] > 0.1:
                stress_viz.append({
                    'point': voxel_data['center'],
                    'stress': voxel_data['stress'],
                    'density': voxel_data['density']
                })
        
        return stress_viz
    
    # ====== THICKNESS METHODS ======
    
    def thicken_lattice(self, min_radius=0.03, max_radius=0.15):
        """
        Create THINNER pipes with variable thickness based on stress
        """
        print("Thickening lattice members...")
        
        thick_lattice = []
        lattice_data = self.generate_lattice()
        
        for item in lattice_data:
            line = item['line']
            density = item['density']
            is_column = item['is_column']
            
            # Calculate radius - now much thinner
            base_radius = min_radius + (density ** 0.5) * (max_radius - min_radius)
            
            # Columns get boost but not too much
            if is_column:
                radius = base_radius * 1.5  # Changed from 1.8
            else:
                radius = base_radius
            
            # Create pipe
            try:
                pipes = rg.Brep.CreatePipe(
                    line, 
                    radius, 
                    False,
                    rg.PipeCapMode.Round,
                    True,
                    0.01,
                    0.01
                )
                
                if pipes:
                    for pipe in pipes:
                        thick_lattice.append({
                            'brep': pipe,
                            'radius': radius,
                            'density': density,
                            'is_column': is_column
                        })
            except:
                pass
        
        print("Created {} thick lattice members".format(len(thick_lattice)))
        return thick_lattice
    
    def create_thick_floors_irregular(self, slab_thickness=0.2):
        """
        Create thick IRREGULAR floor slabs by extruding the irregular shapes
        """
        print("Creating thick irregular floor slabs...")
        
        thick_slabs = []
        floor_data = self.generate_floor_slabs()
        
        for floor_item in floor_data:
            base_brep = floor_item['brep']
            floor_level = floor_item['level']
            height = floor_item['height']
            
            # Extrude the irregular surface downward
            extrusion_vector = rg.Vector3d(0, 0, -slab_thickness)
            
            # Get all faces and extrude them
            extruded_surfaces = []
            for face in base_brep.Faces:
                # Get the outer edge curve
                edge_curves = []
                for edge in face.Edges:
                    edge_curves.append(edge.DuplicateCurve())
                
                if edge_curves:
                    # Join curves to get boundary
                    joined = rg.Curve.JoinCurves(edge_curves, 0.01)
                    if joined and len(joined) > 0:
                        boundary = joined[0]
                        
                        # Extrude to create solid
                        extrusion = rg.Surface.CreateExtrusion(boundary, extrusion_vector)
                        if extrusion:
                            thick_brep = extrusion.ToBrep()
                            if thick_brep:
                                # Cap the brep to make it solid
                                thick_brep.Cap(rg.BrepCapDirection.Both)
                                
                                thick_slabs.append({
                                    'brep': thick_brep,
                                    'level': floor_level,
                                    'height': height,
                                    'thickness': slab_thickness
                                })
        
        print("Created {} thick irregular floor slabs".format(len(thick_slabs)))
        return thick_slabs


# ============================================
# MAIN EXECUTION
# ============================================

# Initialize all outputs
lattice_lines = []
floor_slabs = []
columns = []
stress_field = []
density_map = []
thick_lattice = []
thick_floors = []

print("="*50)
print("TRABECULAR STRUCTURE GENERATOR - REFINED")
print("="*50)

# CREATE DEFAULT BOUNDARY
if not boundary_brep or boundary_brep is None:
    print("No boundary provided - creating default 20x20x30 box")
    box = rg.Box(
        rg.Plane.WorldXY,
        rg.Interval(0, 20),
        rg.Interval(0, 20),
        rg.Interval(0, 30)
    )
    _boundary = box.ToBrep()
else:
    _boundary = boundary_brep
    print("Using provided boundary")

# Adjusted defaults for THINNER, MORE POROUS result
_floor_count = 5
_floor_height = 4.0
_voxel_size = 1.5
_threshold = 0.25  # INCREASED from 0.15 - removes more weak material
_wind_dir = None
_min_thickness = 0.03  # REDUCED from 0.08 - thinner members
_max_thickness = 0.15  # REDUCED from 0.35 - thinner members
_slab_thickness = 0.2  # REDUCED from 0.25 - thinner slabs

# Override with inputs
if 'floor_count' in dir() and floor_count is not None and floor_count > 0:
    _floor_count = int(floor_count)

if 'floor_height' in dir() and floor_height is not None and floor_height > 0:
    _floor_height = float(floor_height)

if 'voxel_size' in dir() and voxel_size is not None and voxel_size > 0:
    _voxel_size = float(voxel_size)

if 'lattice_threshold' in dir() and lattice_threshold is not None:
    _threshold = float(lattice_threshold)

if 'wind_direction' in dir() and wind_direction is not None:
    _wind_dir = wind_direction

print("Floor count: {}".format(_floor_count))
print("Floor height: {}".format(_floor_height))
print("Voxel size: {}".format(_voxel_size))
print("Threshold: {} (HIGHER = more porous)".format(_threshold))
print("Min/Max lattice radius: {}/{}".format(_min_thickness, _max_thickness))
print("Slab thickness: {}".format(_slab_thickness))
print("="*50)

try:
    # Initialize generator
    generator = TrabeculaGenerator(
        _boundary, 
        _floor_count, 
        _floor_height, 
        _voxel_size
    )
    
    # Generate structure
    generator.generate_voxel_grid()
    generator.define_load_points()
    generator.calculate_stress_field(_wind_dir)
    generator.generate_density_map(_threshold)
    generator.identify_floors_and_columns()
    
    # Generate line-based outputs
    lattice_data = generator.generate_lattice()
    floor_data = generator.generate_floor_slabs()
    column_data = generator.extract_columns()
    stress_viz_data = generator.get_stress_visualization()
    
    lattice_lines = [item['line'] for item in lattice_data]
    floor_slabs = [item['brep'] for item in floor_data]
    columns = [item['line'] for item in column_data]
    stress_field = [item['point'] for item in stress_viz_data]
    density_map = [item['point'] for item in stress_viz_data]
    
    print("\n" + "="*50)
    print("CREATING REFINED SOLID GEOMETRY...")
    print("="*50)
    
    # Generate THINNER solid geometry
    thick_lattice_data = generator.thicken_lattice(_min_thickness, _max_thickness)
    thick_lattice = [item['brep'] for item in thick_lattice_data]
    
    # Generate IRREGULAR thick floors
    thick_floor_data = generator.create_thick_floors_irregular(_slab_thickness)
    thick_floors = [item['brep'] for item in thick_floor_data]
    
    print("="*50)
    print("GENERATION COMPLETE!")
    print("="*50)
    print("Lattice lines: {}".format(len(lattice_lines)))
    print("THIN lattice members: {}".format(len(thick_lattice)))
    print("Irregular floor surfaces: {}".format(len(floor_slabs)))
    print("THICK irregular floors: {}".format(len(thick_floors)))
    print("Columns: {}".format(len(columns)))
    print("="*50)

except Exception as e:
    print("ERROR: {}".format(str(e)))
    import traceback
    print(traceback.format_exc())