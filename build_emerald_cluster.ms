resetMaxFile #noPrompt
seed 23
outDir = @"C:\Users\82108\Desktop\EffectPorfolio\Assets\crystal_cluster_asset"
fbxPath = outDir + "\\Emerald_Crystal_Rock_Cluster_3dsMax.fbx"
rockTex = outDir + "\\rock_BaseColor.png"
crystalTex = outDir + "\\crystal_BaseColor.png"

fn makeMat name texPath color metallic roughness =
(
    m = standardMaterial name:name diffuse:color specular:[0.35,0.35,0.35] glossiness:(1.0-roughness)
    if doesFileExist texPath do (m.diffuseMap = bitmapTexture filename:texPath)
    m
)
rockMat = makeMat "Rock_Green_Striped" rockTex [30,65,52] 0.05 0.78
crystalMat = makeMat "Crystal_Emerald_Faceted" crystalTex [28,170,105] 0.22 0.22

fn makeRock name cx cy cz sx sy sz seedOffset =
(
    n=10; rings=4; vv=#(); ff=#();
    for r=0 to rings do
    (
        t=(r as float)/rings; rr=(1.0-0.58*t); 
        for i=0 to n-1 do
        (
            a=2.0*pi*i/n; wob=0.82 + random 0.0 0.30; 
            z=cz + t*sz + (random -0.06 0.06)*sz*(1.0-t);
            append vv [cx + cos a*rr*sx*wob, cy + sin a*rr*sy*wob, z]
        )
    )
    append vv [cx,cy,cz-0.05*sz]; bottom=vv.count
    append vv [cx,cy,cz+sz*1.05]; top=vv.count
    for i=1 to n do
    (
        j=(mod i n)+1; append ff [bottom,j,i]
        for r=0 to rings-1 do
        (
            a=r*n+i; b=r*n+j; c=(r+1)*n+j; d=(r+1)*n+i
            append ff [a,b,c]; append ff [a,c,d]
        )
        append ff [rings*n+i,rings*n+j,top]
    )
    obj=mesh vertices:vv faces:ff name:name; obj.material=rockMat; obj
)

fn makeCrystal name cx cy cz radius height rot =
(
    n=6; vv=#(); ff=#();
    for i=0 to n-1 do (a=rot+2*pi*i/n; append vv [cx+radius*cos a,cy+radius*sin a,cz])
    for i=0 to n-1 do (a=rot+2*pi*i/n; append vv [cx+radius*0.90*cos a,cy+radius*0.90*sin a,cz+height*0.56])
    append vv [cx,cy,cz+height]; tip=vv.count
    append vv [cx,cy,cz-0.02]; bottom=vv.count
    for i=1 to n do
    (
        j=(mod i n)+1; append ff [i,j,n+j]; append ff [i,n+j,n+i]; append ff [n+i,n+j,tip]; append ff [bottom,j,i]
    )
    obj=mesh vertices:vv faces:ff name:name; obj.material=crystalMat; obj
)

allObjs=#()
append allObjs (makeRock "Rock_Base" 0 0 0 1.55 1.28 2.15 1)
append allObjs (makeRock "Rock_Left" -0.72 0.05 0.18 0.78 0.68 1.40 2)
append allObjs (makeRock "Rock_Right" 0.72 0.08 0.12 0.82 0.70 1.52 3)
append allObjs (makeRock "Rock_Back" 0.0 0.34 0.42 0.96 0.65 1.56 4)

append allObjs (makeCrystal "Crystal_01" 0.00 0.10 1.58 0.48 2.35 0.10)
append allObjs (makeCrystal "Crystal_02" -0.48 0.00 1.12 0.28 1.58 0.35)
append allObjs (makeCrystal "Crystal_03" 0.52 0.05 1.20 0.30 1.72 0.62)
append allObjs (makeCrystal "Crystal_04" -0.82 -0.02 0.78 0.24 1.20 0.20)
append allObjs (makeCrystal "Crystal_05" 0.84 0.00 0.82 0.25 1.28 0.44)
append allObjs (makeCrystal "Crystal_06" -0.24 -0.52 0.82 0.24 1.10 0.10)
append allObjs (makeCrystal "Crystal_07" 0.28 -0.48 0.75 0.22 0.98 0.50)
append allObjs (makeCrystal "Crystal_08" -1.02 0.18 0.48 0.18 0.78 0.10)
append allObjs (makeCrystal "Crystal_09" 1.04 0.20 0.54 0.18 0.88 0.65)
append allObjs (makeCrystal "Crystal_10" -0.62 0.32 1.22 0.18 0.94 0.30)
append allObjs (makeCrystal "Crystal_11" 0.62 0.36 1.25 0.20 1.04 0.72)
append allObjs (makeCrystal "Crystal_12" -0.32 -0.20 1.52 0.16 0.84 0.05)
append allObjs (makeCrystal "Crystal_13" 0.34 -0.18 1.45 0.16 0.92 0.58)
append allObjs (makeCrystal "Crystal_14" -1.18 -0.10 0.28 0.13 0.55 0.30)
append allObjs (makeCrystal "Crystal_15" 1.18 -0.04 0.34 0.14 0.62 0.70)
append allObjs (makeCrystal "Crystal_16" -0.10 0.46 1.38 0.14 0.70 0.12)
append allObjs (makeCrystal "Crystal_17" 0.18 0.48 1.56 0.13 0.76 0.72)
append allObjs (makeCrystal "Crystal_18" -0.92 0.35 0.36 0.12 0.55 0.42)
append allObjs (makeCrystal "Crystal_19" 0.92 0.36 0.42 0.12 0.60 0.78)
append allObjs (makeCrystal "Crystal_20" -0.40 0.56 0.72 0.11 0.48 0.20)
append allObjs (makeCrystal "Crystal_21" 0.42 0.58 0.78 0.11 0.52 0.62)
select allObjs
FBXExporterSetParam "ASCII" false
FBXExporterSetParam "Animation" false
FBXExporterSetParam "Cameras" false
FBXExporterSetParam "Lights" false
FBXExporterSetParam "EmbedTextures" false
exportFile fbxPath #noPrompt selectedOnly:true using:FBXEXP
format "CREATED_CLUSTER_OBJECTS=%\n" allObjs.count
format "CREATED_CLUSTER_FBX=%\n" fbxPath
