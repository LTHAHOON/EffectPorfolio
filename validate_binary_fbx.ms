resetMaxFile #noPrompt
inputPath = @"C:\Users\82108\Desktop\EffectPorfolio\Assets\crystal_cluster_asset\Emerald_Crystal_Rock_Cluster_3dsMax.fbx"
importFile inputPath #noPrompt
format "FBX_REIMPORT_NODE_COUNT=%\n" objects.count
for obj in objects do
(
    format "FBX_REIMPORT_NODE=% CLASS=% FACES=%\n" obj.name (classOf obj) (getNumFaces obj)
)
if objects.count == 0 then throw "FBX reimport contained no mesh objects"
