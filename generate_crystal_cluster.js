const fs = require('fs');
const zlib = require('zlib');
const path = require('path');

const outDir = path.join(process.cwd(), 'crystal_cluster_asset');
fs.mkdirSync(outDir, { recursive: true });

function png(width, height, fn) {
  const raw = Buffer.alloc((width * 4 + 1) * height);
  for (let y = 0; y < height; y++) {
    raw[y * (width * 4 + 1)] = 0;
    for (let x = 0; x < width; x++) {
      const c = fn(x, y);
      const i = y * (width * 4 + 1) + 1 + x * 4;
      raw[i] = c[0]; raw[i+1] = c[1]; raw[i+2] = c[2]; raw[i+3] = c[3] ?? 255;
    }
  }
  const crc = b => { let n = 0xffffffff; for (const v of b) { n ^= v; for (let k=0;k<8;k++) n = (n>>>1) ^ (n&1 ? 0xedb88320 : 0); } return (n ^ 0xffffffff) >>> 0; };
  const chunk = (type, data) => { const t=Buffer.from(type); const o=Buffer.alloc(12+data.length); o.writeUInt32BE(data.length,0); t.copy(o,4); data.copy(o,8); o.writeUInt32BE(crc(Buffer.concat([t,data])),8+data.length); return o; };
  const ih=Buffer.alloc(13); ih.writeUInt32BE(width,0); ih.writeUInt32BE(height,4); ih[8]=8; ih[9]=6;
  return Buffer.concat([Buffer.from([137,80,78,71,13,10,26,10]), chunk('IHDR',ih), chunk('IDAT',zlib.deflateSync(raw,{level:6})), chunk('IEND',Buffer.alloc(0))]);
}
const W=512,H=512;
function hash(x,y){ const n=Math.sin(x*12.9898+y*78.233)*43758.5453; return n-Math.floor(n); }
for (const [name,kind] of [['crystal',0],['rock',1]]) {
  const base = png(W,H,(x,y)=>{
    const n=(hash(x*0.9,y*1.1)+hash(x*2.7,y*2.1)*0.45)/1.45;
    if(kind===0){ const f=0.5+0.5*Math.sin((x+y)*0.045); return [18+Math.floor(22*n),90+Math.floor(90*n+28*f),66+Math.floor(75*n+18*f),255]; }
    const stripe=Math.pow(Math.max(0,Math.sin(x*0.038+y*0.014+Math.sin(y*0.02)*1.4)),18);
    return [22+Math.floor(24*n+12*stripe),38+Math.floor(33*n+32*stripe),31+Math.floor(30*n+28*stripe),255];
  });
  fs.writeFileSync(path.join(outDir,`${name}_BaseColor.png`),base);
  const normal = png(W,H,(x,y)=>{ const n=hash(x*1.8,y*1.5); const ridge=kind?Math.pow(Math.max(0,Math.sin(x*0.038+y*0.014)),14):0.5+0.5*Math.sin((x-y)*0.08); return [128+Math.floor((n-.5)*18),128+Math.floor((ridge-.5)*45),255-Math.floor((n-.5)*12),255]; });
  fs.writeFileSync(path.join(outDir,`${name}_Normal.png`),normal);
  const rough = png(W,H,()=>{ const n=hash(Math.random()*W,Math.random()*H); return kind?[Math.floor(150+70*n),Math.floor(150+70*n),Math.floor(150+70*n),255]:[70,70,70,255]; });
  fs.writeFileSync(path.join(outDir,`${name}_Roughness.png`),rough);
  const metal = png(W,H,()=>kind?[12,12,12,255]:[35,35,35,255]);
  fs.writeFileSync(path.join(outDir,`${name}_Metallic.png`),metal);
}

const verts=[], faces=[], uvs=[];
function addMesh(vs, fs, mat, uvScale=1){ const base=verts.length; for(const v of vs) verts.push(v); for(const f of fs){ faces.push({idx:f.map(i=>i+base),mat}); for(const i of f){ const v=vs[i]; uvs.push([0.5+v[0]*uvScale,0.5+v[2]*uvScale]); } } }
function crystal(cx,cy,cz,r,h,rot){
  const n=6, vs=[]; for(let i=0;i<n;i++){const a=rot+i*Math.PI*2/n; vs.push([cx+r*Math.cos(a),cy,cz+r*Math.sin(a)]);} for(let i=0;i<n;i++){const a=rot+i*Math.PI*2/n; vs.push([cx+r*.9*Math.cos(a),cy+h*.58,cz+r*.9*Math.sin(a)]);} vs.push([cx,cy+h,cz]); const fs=[]; for(let i=0;i<n;i++){const j=(i+1)%n;fs.push([i,j,n+j,n+i]);fs.push([n+i,n+j,2*n]);} fs.push([...Array(n).keys()].reverse()); addMesh(vs,fs,0,0.22);
}
// Base: faceted, slightly asymmetrical low-poly rock
const rv=[]; const rings=4, seg=12; for(let y=0;y<=rings;y++){const t=y/rings, rr=(1.1-0.65*t)*(0.9+0.12*Math.sin(y*2)); for(let i=0;i<seg;i++){const a=i*Math.PI*2/seg; const wob=0.9+0.12*Math.sin(i*2.7+y*1.4); rv.push([rr*wob*Math.cos(a), t*2.45-1.05, rr*wob*Math.sin(a)]);}} const rf=[]; for(let y=0;y<rings;y++)for(let i=0;i<seg;i++){const j=(i+1)%seg;rf.push([y*seg+i,y*seg+j,(y+1)*seg+j,(y+1)*seg+i]);} rf.push([...Array(seg).keys()].reverse()); addMesh(rv,rf,1,0.3);
const specs=[[0,0.95,0.25,.42,1.8],[0.55,0.18,0.2,.3,1.25],[-0.55,0.22,0.1,.3,1.35],[0.25,-0.2,0.48,.24,1.0],[-0.28,-0.25,-0.2,.22,.95],[0.72,-0.3,-0.1,.18,.75],[-.72,-.35,.2,.2,.85],[0.15,-.52,-.45,.16,.62],[-.42,-.5,-.45,.14,.52]];
for(const [x,y,z,r,h] of specs) crystal(x,y,z,r,h,(x+z)*2.1);

function fbxArray(a){return a.join(',');}
let fbx='; FBX 7.4.0 project file\nFBXHeaderExtension:  { FBXHeaderVersion: 1003, FBXVersion: 7400 }\nGlobalSettings:  { Version: 1000, Properties70:  { P: "UpAxis", "int", "Integer", "",1 P: "UpAxisSign", "int", "Integer", "",1 P: "FrontAxis", "int", "Integer", "",2 P: "FrontAxisSign", "int", "Integer", "",1 P: "CoordAxis", "int", "Integer", "",0 P: "CoordAxisSign", "int", "Integer", "",1 P: "UnitScaleFactor", "double", "Number", "",100 } }\nObjects:  {\n';
const geomId=100000, modelId=200000, matRock=300000, matCrystal=300001, texBaseR=400000, texNormR=400001, texRoughR=400002, texMetalR=400003, texBaseC=400010, texNormC=400011, texRoughC=400012, texMetalC=400013;
const tris=[]; for(const f of faces){ for(let i=1;i<f.idx.length-1;i++) tris.push({idx:[f.idx[0],f.idx[i],f.idx[i+1]],mat:f.mat}); }
const triUv=tris.flatMap(f=>f.idx.flatMap(v=>{const p=verts[v]; return [0.5+p[0]*0.22,0.5+p[2]*0.22];}));
fbx='; FBX 7.4.0 project file\nFBXHeaderExtension:  { FBXVersion: 7400 }\nGlobalSettings:  { Version: 1000 Properties70:  { P: "UpAxis", "int", "Integer", "",1 P: "UpAxisSign", "int", "Integer", "",1 P: "FrontAxis", "int", "Integer", "",2 P: "FrontAxisSign", "int", "Integer", "",1 P: "CoordAxis", "int", "Integer", "",0 P: "CoordAxisSign", "int", "Integer", "",1 } }\nDefinitions:  { Version: 100 ObjectType: "Model" { Count: 1 } ObjectType: "Geometry" { Count: 1 } ObjectType: "Material" { Count: 2 } ObjectType: "Texture" { Count: 8 } ObjectType: "Video" { Count: 8 } }\nObjects:  {\n';
fbx+=`Geometry: ${geomId}, "Geometry::CrystalRockCluster", "Mesh" { GeometryVersion: 124\n Vertices: *${verts.length*3} { a: ${fbxArray(verts.flat())} }\n PolygonVertexIndex: *${tris.length*3} { a: ${fbxArray(tris.flatMap(f=>f.idx.map((v,i)=>i===2?-(v+1):v))) } }\n LayerElementUV: 0 { Version: 101 Name: "UVChannel_1" MappingInformationType: "ByPolygonVertex" ReferenceInformationType: "IndexToDirect" UV: *${triUv.length} { a: ${fbxArray(triUv)} } UVIndex: *${triUv.length/2} { a: ${fbxArray(triUv.map((_,i)=>Math.floor(i/2)))} } }\n LayerElementMaterial: { Version: 101 MappingInformationType: "ByPolygon" ReferenceInformationType: "IndexToDirect" Materials: *${tris.length} { a: ${fbxArray(tris.map(f=>f.mat))} } }\n}\n`;
fbx+=`Model: ${modelId}, "Model::CrystalRockCluster", "Mesh" { Version: 232 Properties70: { P: "ShadingModel", "KString", "", "", "phong" P: "Culling", "enum", "", "", 0 } }\n`;
function mat(id,name,color,metal,rough){return `Material: ${id}, "Material::${name}", "" { Version: 102 ShadingModel: "PBR" MultiLayer: 0 Properties70: { P: "DiffuseColor", "Color", "", "A",${color.join(',')} P: "Metalness", "double", "Number", "",${metal} P: "Roughness", "double", "Number", "",${rough} } }\n`;}
fbx+=mat(matRock,'Rock',[0.12,0.28,0.22],0.05,0.78)+mat(matCrystal,'Crystal',[0.05,0.5,0.3],0.2,0.22);
function tex(id,name,file){return `Texture: ${id}, "Texture::${name}", "" { Type: "TextureVideoClip" FileName: "${file}" RelativeFilename: "${file}" }\n`;}
for(const [id,n,f] of [[texBaseR,'rock_BaseColor','rock_BaseColor.png'],[texNormR,'rock_Normal','rock_Normal.png'],[texRoughR,'rock_Roughness','rock_Roughness.png'],[texMetalR,'rock_Metallic','rock_Metallic.png'],[texBaseC,'crystal_BaseColor','crystal_BaseColor.png'],[texNormC,'crystal_Normal','crystal_Normal.png'],[texRoughC,'crystal_Roughness','crystal_Roughness.png'],[texMetalC,'crystal_Metallic','crystal_Metallic.png']]) fbx+=tex(id,n,f);
fbx+='}\nConnections:  {\n';
fbx+=`C: "OO",${geomId},${modelId}\nC: "OO",${matRock},${modelId}\nC: "OO",${matCrystal},${modelId}\n`;
for(const [tid,mid,prop] of [[texBaseR,matRock,'DiffuseColor'],[texNormR,matRock,'NormalMap'],[texRoughR,matRock,'Roughness'],[texMetalR,matRock,'Metalness'],[texBaseC,matCrystal,'DiffuseColor'],[texNormC,matCrystal,'NormalMap'],[texRoughC,matCrystal,'Roughness'],[texMetalC,matCrystal,'Metalness']]) fbx+=`C: "OP",${tid},${mid},"${prop}"\n`;
fbx+='}\nTakes:  { Current: "" }\n';
fs.writeFileSync(path.join(outDir,'Emerald_Crystal_Rock_Cluster.fbx'),fbx);
let obj='mtllib Emerald_Crystal_Rock_Cluster.mtl\no Emerald_Crystal_Rock_Cluster\n';
for(const v of verts) obj+=`v ${v[0]} ${v[1]} ${v[2]}\n`;
const uvFor=(v)=>{const p=verts[v];return [0.5+p[0]*0.22,0.5+p[2]*0.22];};
for(const f of tris) for(const v of f.idx){const uv=uvFor(v); obj+=`vt ${uv[0]} ${uv[1]}\n`;}
let uvCursor=1, current=-1;
for(const f of tris){if(f.mat!==current){current=f.mat;obj+=`usemtl ${current===0?'Crystal':'Rock'}\n`;}obj+='f '+f.idx.map(v=>`${v+1}/${uvCursor++}`).join(' ')+'\n';}
fs.writeFileSync(path.join(outDir,'Emerald_Crystal_Rock_Cluster.obj'),obj);
fs.writeFileSync(path.join(outDir,'Emerald_Crystal_Rock_Cluster.mtl'),'newmtl Crystal\nKd 0.05 0.5 0.3\nmap_Kd crystal_BaseColor.png\n\nnewmtl Rock\nKd 0.12 0.28 0.22\nmap_Kd rock_BaseColor.png\n');
fs.writeFileSync(path.join(outDir,'README_Unity.txt'),`Unity-ready asset generated as an ASCII FBX.\nImport Emerald_Crystal_Rock_Cluster.fbx and place the PNG maps beside it.\nCrystal material: green emerald faceted crystal, metallic 0.2, roughness 0.22.\nRock material: dark green stone with teal recessed stripe pattern, roughness 0.78.\nApproximate triangles: ${faces.reduce((s,f)=>s+f.idx.length-2,0)}.\n\nMap assignment:\nCrystal: crystal_BaseColor.png -> Albedo, crystal_Normal.png -> Normal, crystal_Metallic.png -> Metallic, crystal_Roughness.png -> Smoothness (invert in Unity if needed).\nRock: rock_BaseColor.png -> Albedo, rock_Normal.png -> Normal, rock_Metallic.png -> Metallic, rock_Roughness.png -> Smoothness (invert in Unity if needed).\nAll maps are 512 x 512 PNG.\n`);
console.log(JSON.stringify({outDir,vertices:verts.length,polygons:faces.length,triangles:faces.reduce((s,f)=>s+f.idx.length-2,0)}));
