resetMaxFile #noPrompt
inputPath = @"C:\Users\82108\Desktop\EffectPorfolio\crystal_cluster_asset\Emerald_Crystal_Rock_Cluster.obj"
outputPath = @"C:\Users\82108\Desktop\EffectPorfolio\crystal_cluster_asset\Emerald_Crystal_Rock_Cluster_binary.fbx"
importFile inputPath #noPrompt
format "Imported nodes: %\n" objects.count
FBXExporterSetParam "ASCII" false
FBXExporterSetParam "Animation" false
FBXExporterSetParam "Cameras" false
FBXExporterSetParam "Lights" false
FBXExporterSetParam "EmbedTextures" false
exportFile outputPath #noPrompt selectedOnly:false using:FBXEXP
format "Exported: %\n" outputPath
