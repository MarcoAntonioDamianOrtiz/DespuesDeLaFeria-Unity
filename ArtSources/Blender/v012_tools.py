"""Original feria tools, metres. Run in a separate Blender factory-startup process."""
import bpy, math, random, json
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'Assets/Feria/Art/Original/ToolsV012'
SRC=Path(__file__).resolve().parent/'v012'
OUT.mkdir(parents=True,exist_ok=True);SRC.mkdir(parents=True,exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
def mat(n,c,rough=.6,metal=0):
    m=bpy.data.materials.new(n);m.diffuse_color=(*c,1);m.use_nodes=True
    p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=(*c,1);p.inputs['Roughness'].default_value=rough;p.inputs['Metallic'].default_value=metal
    return m
wood=mat('Madera miel',(.40,.20,.075));teal=mat('Esmalte petroleo',(.04,.25,.24),.38);brass=mat('Laton',(.64,.43,.13),.3,.4)
fibres=[mat('Cerdas '+str(i),c) for i,c in enumerate([(.68,.46,.16),(.87,.67,.31),(.51,.32,.10)])]
cloth=mat('Tela salvia',(.28,.51,.43),.95);hem=mat('Dobladillo marfil',(.84,.79,.59),.95)
def cube(n,loc,scale,m,bevel=0):
    bpy.ops.mesh.primitive_cube_add(size=1,location=loc);o=bpy.context.object;o.name=n;o.dimensions=scale;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);o.data.materials.append(m)
    if bevel:
        mod=o.modifiers.new('Cantos suaves','BEVEL');mod.width=bevel;mod.segments=2;bpy.ops.object.modifier_apply(modifier=mod.name)
    return o
def cone(n,loc,r1,r2,depth,m,verts=8):
    bpy.ops.mesh.primitive_cone_add(vertices=verts,radius1=r1,radius2=r2,depth=depth,location=loc);o=bpy.context.object;o.name=n;o.data.materials.append(m);return o
def export(name,objects):
    bpy.ops.object.select_all(action='DESELECT')
    for o in objects:o.select_set(True)
    bpy.context.view_layer.objects.active=objects[0];bpy.ops.object.join();o=bpy.context.object;o.name=name
    bpy.context.scene.cursor.location=(0,0,0);bpy.ops.object.origin_set(type='ORIGIN_CURSOR')
    bpy.ops.export_scene.fbx(filepath=str(OUT/(name+'.fbx')),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,bake_space_transform=True,add_leaf_bones=False)
    return o
before=set(bpy.data.objects)
cube('Cabezal', (0,0,.22),(.54,.16,.075),teal,.012)
cube('Lomo de laton',(0,0,.263),(.46,.12,.012),brass,.004)
cone('Mango',(0,0,.91),.018,.014,1.28,wood,10)
cone('Empunadura',(0,0,1.51),.024,.022,.20,teal,10)
cone('Casquillo',(0,0,.32),.031,.028,.08,brass,10)
random.seed(1202)
for x in range(24):
    for y in range(3):
        length=.18+random.random()*.035
        o=cone('Haz de cerdas',((x-11.5)*.021,(y-1)*.048,.205-length/2),.0055,.0085,length,fibres[(x+y)%3],5)
        o.rotation_euler=(random.uniform(-.08,.08),random.uniform(-.06,.06),0)
broom=export('Escoba',list(set(bpy.data.objects)-before))
before=set(bpy.data.objects);verts=[];faces=[];nx=17;ny=15
for y in range(ny):
    for x in range(nx):
        u=x/(nx-1);v=y/(ny-1)
        verts.append(((u-.5)*.36+.008*math.sin(v*15),(v-.5)*.29+.009*math.sin(u*19),.008*math.sin(u*14+v*3)+.006*math.cos(v*17)+.004))
for y in range(ny-1):
    for x in range(nx-1):
        i=y*nx+x;faces.append((i,i+1,i+1+nx,i+nx))
mesh=bpy.data.meshes.new('Tela plegada');mesh.from_pydata(verts,[],faces);mesh.materials.append(cloth);mesh.materials.append(hem)
o=bpy.data.objects.new('Tela',mesh);bpy.context.collection.objects.link(o)
for p in mesh.polygons:
    cx=sum(verts[i][0] for i in p.vertices)/4;cy=sum(verts[i][1] for i in p.vertices)/4
    if abs(cx)>.15 or abs(cy)>.115:p.material_index=1
    p.use_smooth=True
bpy.context.view_layer.objects.active=o;o.select_set(True)
mod=o.modifiers.new('Espesor textil','SOLIDIFY');mod.thickness=.0025;bpy.ops.object.modifier_apply(modifier=mod.name)
rag=export('Pano',list(set(bpy.data.objects)-before))
bpy.ops.wm.save_as_mainfile(filepath=str(SRC/'Herramientas.blend'))
receipt={}
for o in [broom,rag]:
    o.data.calc_loop_triangles();receipt[o.name]={'triangles':len(o.data.loop_triangles),'dimensions':list(o.dimensions),'materials':len(o.data.materials)}
(SRC/'receipt.json').write_text(json.dumps(receipt,indent=2))
print(json.dumps(receipt))
