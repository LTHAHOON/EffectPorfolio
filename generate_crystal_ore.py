import math, random, os
from pathlib import Path
from PIL import Image, ImageDraw, ImageFilter

OUT = Path('assets/crystal_ore')
OUT.mkdir(parents=True, exist_ok=True)
random.seed(21)

verts=[]; faces=[]; mats=[]
def add_tri(a,b,c,m): faces.append((a,b,c)); mats.append(m)
def add_rock():
    seg=12; rings=[(0.0,1.05),(0.32,1.30),(0.9,1.18),(1.48,0.92),(1.95,0.56)]
    ring_ids=[]
    for ri,(z,r) in enumerate(rings):
        ids=[]
        for s in range(seg):
            a=2*math.pi*s/seg
            wob=1+0.10*math.sin(3*a+ri)+0.06*random.uniform(-1,1)
            ids.append(len(verts)); verts.append((r*wob*math.cos(a), r*wob*math.sin(a), z+0.05*math.sin(2*a+ri)))
        ring_ids.append(ids)
    # bottom and top caps
    b=len(verts); verts.append((0,0,-0.08)); t=len(verts); verts.append((0,0,2.12))
    for s in range(seg): add_tri(b,ring_ids[0][(s+1)%seg],ring_ids[0][s],0)
    for ri in range(len(ring_ids)-1):
        A=ring_ids[ri]; B=ring_ids[ri+1]
        for s in range(seg):
            a=A[s]; an=A[(s+1)%seg]; bb=B[s]; bn=B[(s+1)%seg]
            add_tri(a,an,bn,0); add_tri(a,bn,bb,0)
    for s in range(seg): add_tri(ring_ids[-1][s],ring_ids[-1][(s+1)%seg],t,0)
def add_crystal(x,y,z,h,r,rot=0):
    n=6; base=[]; mid=[]
    for j in range(n):
        a=rot+2*math.pi*j/n; base.append(len(verts)); verts.append((x+r*math.cos(a),y+r*math.sin(a),z))
    for j in range(n):
        a=rot+2*math.pi*j/n; mid.append(len(verts)); verts.append((x+r*0.88*math.cos(a),y+r*0.88*math.sin(a),z+h*0.64))
    tip=len(verts); verts.append((x,y,z+h))
    for j in range(n):
        k=(j+1)%n
        add_tri(base[j],base[k],mid[k],1); add_tri(base[j],mid[k],mid[j],1)
        add_tri(mid[j],mid[k],tip,1)
    # cap
    center=len(verts); verts.append((x,y,z-0.01))
    for j in range(n): add_tri(center,base[j],base[(j+1)%n],1)

add_rock()
for p in [(0.0,0.05,1.05,1.12,0.32,0.2),(-0.48,0.08,0.62,0.84,0.22,0.5),(0.43,0.18,0.74,0.92,0.25,0.1),(-0.12,-0.30,0.42,0.62,0.18,0.1),(0.58,-0.18,1.08,0.62,0.18,0.4),(-0.58,-0.18,1.15,0.54,0.17,0.2),(0.18,0.32,1.38,0.48,0.16,0.0),(0.78,0.1,0.42,0.45,0.16,0.4),(-0.82,0.08,0.38,0.42,0.14,0.2)]: add_crystal(*p)

def texmaps():
    n=512
    base=Image.new('RGB',(n,n),(28,43,38)); px=base.load()
    for y in range(n):
        for x in range(n):
            v=random.randint(-10,10); px[x,y]=(max(0,25+v),max(0,47+v),max(0,40+v))
    d=ImageDraw.Draw(base)
    for i in range(28):
        pts=[]; x=random.randrange(n); y=random.randrange(n)
        for k in range(random.randrange(3,7)):
            pts.append((x,y)); x=max(0,min(n-1,x+random.randrange(-90,91))); y=max(0,min(n-1,y+random.randrange(-70,71)))
        d.line(pts,fill=(30,155+random.randrange(60),95+random.randrange(70)),width=random.randrange(3,9),joint='curve')
    base=base.filter(ImageFilter.GaussianBlur(0.7)); base.save(OUT/'crystal_ore_basecolor.png')
    crystal=Image.new('RGB',(n,n)); cp=crystal.load()
    for y in range(n):
        for x in range(n):
            band=((x//38 + y//52) % 3)
            cp[x,y]=((18+band*8), (118+band*22), (78+band*24))
    crystal=crystal.filter(ImageFilter.GaussianBlur(1.2)); crystal.save(OUT/'crystal_ore_crystal_basecolor.png')
    normal=Image.new('RGB',(n,n),(128,128,255)); nd=ImageDraw.Draw(normal)
    for i in range(36):
        pts=[]; x=random.randrange(n); y=random.randrange(n)
        for k in range(random.randrange(2,6)):
            pts.append((x,y)); x=max(0,min(n-1,x+random.randrange(-85,86))); y=max(0,min(n-1,y+random.randrange(-70,71)))
        nd.line(pts,fill=(105,160,220),width=random.randrange(2,6))
    normal.save(OUT/'crystal_ore_normal.png')
    Image.new('L',(n,n),70).save(OUT/'crystal_ore_roughness.png')
    Image.new('L',(n,n),25).save(OUT/'crystal_ore_metallic.png')
texmaps()

def fbx_str():
    # ASCII FBX with one mesh, two material slots, UVs, and linked base/normal textures.
    pv=[]
    for v in verts: pv += [v[0],v[1],v[2]]
    pvi=[]
    for a,b,c in faces: pvi += [a,b,-c-1]
    uv=[]
    for a,b,c in faces:
        for i in (a,b,c):
            x,y,z=verts[i]; uv += [(x+1.5)/3.0,(y+1.5)/3.0]
    uv_idx=list(range(len(uv)//2))
    def arr(vals): return ','.join(f'{v:.6f}' if isinstance(v,float) else str(v) for v in vals)
    lines=['; FBX 7.4.0 project file','FBXHeaderExtension:  { FBXVersion: 7400 }','GlobalSettings:  {','  Version: 1000','  Properties70:  { P: "UpAxis", "int", "Integer", "",1 P: "UpAxisSign", "int", "Integer", "",1 P: "FrontAxis", "int", "Integer", "",2 P: "FrontAxisSign", "int", "Integer", "",1 P: "CoordAxis", "int", "Integer", "",0 P: "CoordAxisSign", "int", "Integer", "",1 }','}','Definitions:  { Version: 100 ObjectType: "Model" { Count: 1 } ObjectType: "Geometry" { Count: 1 } ObjectType: "Material" { Count: 2 } ObjectType: "Texture" { Count: 2 } ObjectType: "Video" { Count: 2 } }','Objects:  {']
    lines.append('Geometry: 1001, "Geometry::CrystalOre", "Mesh" {'); lines.append('  GeometryVersion: 124'); lines.append('  Vertices: *%d { a: %s }'%(len(pv),arr(pv))); lines.append('  PolygonVertexIndex: *%d { a: %s }'%(len(pvi),arr(pvi)))
    lines.append('  LayerElementMaterial:  { Version: 101 MappingInformationType: "ByPolygon" ReferenceInformationType: "IndexToDirect" Materials: *%d { a: %s } }'%(len(mats),arr(mats)))
    lines.append('  LayerElementUV: 0 { Version: 101 Name: "UVChannel_1" MappingInformationType: "ByPolygonVertex" ReferenceInformationType: "IndexToDirect" UV: *%d { a: %s } UVIndex: *%d { a: %s } }'%(len(uv),arr(uv),len(uv_idx),arr(uv_idx))); lines.append('}')
    lines.append('Model: 1002, "Model::CrystalOre", "Mesh" { Version: 232 Properties70: { P: "ShadingModel", "KString", "", "", "phong" P: "Culling", "enum", "", "", 0 } }')
    lines.append('Material: 2001, "Material::Rock", "" { Version: 102 ShadingModel: "phong" Properties70: { P: "DiffuseColor", "Color", "", "A", 0.06,0.12,0.10 P: "Roughness", "double", "Number", "", 0.82 } }')
    lines.append('Material: 2002, "Material::Crystal", "" { Version: 102 ShadingModel: "phong" Properties70: { P: "DiffuseColor", "Color", "", "A", 0.02,0.42,0.18 P: "EmissiveColor", "Color", "", "A", 0.01,0.12,0.04 P: "Roughness", "double", "Number", "", 0.22 } }')
    for tid,name,file in [(3001,'RockBaseColor','crystal_ore_basecolor.png'),(3002,'Normal','crystal_ore_normal.png'),(3003,'CrystalBaseColor','crystal_ore_crystal_basecolor.png')]:
        lines.append(f'Texture: {tid}, "Texture::{name}", "" {{ Type: "TextureVideoClip" TextureName: "{name}" FileName: "{file}" RelativeFileName: "{file}" }}')
    lines.append('Connections:  { C: "OO", 1001, 1002 C: "OO", 2001, 1002 C: "OO", 2002, 1002 C: "OP", 3001, 2001, "DiffuseColor" C: "OP", 3003, 2002, "DiffuseColor" C: "OP", 3002, 2001, "NormalMap" C: "OP", 3002, 2002, "NormalMap" }')
    lines.append('}')
    return '\n'.join(lines)

(OUT/'crystal_ore.fbx').write_text(fbx_str(),encoding='utf-8')
print(f'Wrote {OUT}/crystal_ore.fbx with {len(verts)} vertices and {len(faces)} triangles')
