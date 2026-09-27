resetMaxFile #noPrompt
seed 42
outDir = @"C:\Users\82108\Desktop\EffectPorfolio\Assets\crystal_cluster_asset"
fbxPath = outDir + "\\Emerald_Crystal_Rock_Cluster_FINAL.fbx"
rockTex = outDir + "\\rock_BaseColor.png"
crystalTex = outDir + "\\crystal_BaseColor.png"

fn materialWithMap nm tex col gloss =
(
 m=standardMaterial name:nm diffuse:col glossiness:gloss
 if doesFileExist tex do m.diffuseMap=bitmapTexture filename:tex
 m
)
rockMat=materialWithMap "Rock_Striped_Green" rockTex [30,65,52] 18
crystalMat=materialWithMap "Crystal_Emerald" crystalTex [28,170,105] 78
objs=#()

fn addRock nm p sc rot =
(
 g=geosphere name:nm radius:1.0 segments:2 pos:p isSelected:false
 g.scale=sc; g.rotation=eulerAngles rot[1] rot[2] rot[3]; g.material=rockMat; convertToMesh g; append objs g; g
)
addRock "Rock_Main" [0,0,1.0] [1.50,1.20,2.15] [0,0,0]
addRock "Rock_Left" [-0.72,0.02,0.72] [0.82,0.72,1.48] [0,8,-8]
addRock "Rock_Right" [0.74,0.04,0.78] [0.84,0.70,1.55] [0,-6,12]
addRock "Rock_Back" [0,0.30,1.02] [1.05,0.72,1.72] [4,0,0]
addRock "Rock_Front" [0,-0.42,0.54] [1.12,0.68,0.82] [0,0,0]

fn addCrystal nm p r h rot =
(
 base=cone name:(nm+"_Body") radius1:r radius2:(r*0.82) height:(h*0.62) sides:6 cap:true pos:[p[1],p[2],p[3]+h*0.31]
 base.rotation=eulerAngles 0 0 rot; base.material=crystalMat; convertToMesh base; append objs base
 tip=cone name:(nm+"_Tip") radius1:(r*0.82) radius2:0 height:(h*0.38) sides:6 cap:true pos:[p[1],p[2],p[3]+h*0.81]
 tip.rotation=eulerAngles 0 0 rot; tip.material=crystalMat; convertToMesh tip; append objs tip
)
addCrystal "Crystal_01" [0,0.18,2.15] 0.52 2.55 0
addCrystal "Crystal_02" [-0.52,0.02,1.62] 0.30 1.72 -12
addCrystal "Crystal_03" [0.55,0.02,1.70] 0.32 1.85 11
addCrystal "Crystal_04" [-0.88,0.00,1.12] 0.25 1.30 -10
addCrystal "Crystal_05" [0.88,0.02,1.16] 0.26 1.38 13
addCrystal "Crystal_06" [-0.32,-0.48,1.08] 0.23 1.18 -5
addCrystal "Crystal_07" [0.34,-0.46,1.02] 0.22 1.08 8
addCrystal "Crystal_08" [-1.12,0.08,0.72] 0.18 0.88 -15
addCrystal "Crystal_09" [1.12,0.10,0.76] 0.19 0.96 14
addCrystal "Crystal_10" [-0.68,0.28,1.58] 0.19 1.04 -8
addCrystal "Crystal_11" [0.68,0.30,1.60] 0.20 1.12 12
addCrystal "Crystal_12" [-0.18,-0.10,2.00] 0.17 0.96 -4
addCrystal "Crystal_13" [0.22,-0.12,1.92] 0.17 1.02 5
addCrystal "Crystal_14" [-1.28,-0.10,0.52] 0.14 0.62 -22
addCrystal "Crystal_15" [1.27,-0.08,0.54] 0.15 0.68 20
addCrystal "Crystal_16" [-0.12,0.48,1.86] 0.14 0.80 -5
addCrystal "Crystal_17" [0.20,0.50,2.00] 0.14 0.86 9
addCrystal "Crystal_18" [-0.94,0.34,0.72] 0.13 0.62 -13
addCrystal "Crystal_19" [0.96,0.36,0.78] 0.13 0.68 16
addCrystal "Crystal_20" [-0.44,0.56,1.05] 0.12 0.52 -8
addCrystal "Crystal_21" [0.46,0.56,1.10] 0.12 0.56 12

select objs
FBXExporterSetParam "ASCII" false
FBXExporterSetParam "Animation" false
FBXExporterSetParam "Cameras" false
FBXExporterSetParam "Lights" false
FBXExporterSetParam "EmbedTextures" false
exportFile fbxPath #noPrompt selectedOnly:true using:FBXEXP
format "FINAL_FBX=%\n" fbxPath
