resetMaxFile #noPrompt
inputPath = @"C:\Users\82108\Desktop\EffectPorfolio\Assets\crystal_cluster_asset\Emerald_Crystal_Rock_Cluster_FINAL.fbx"
renderPath = @"C:\Users\82108\Desktop\EffectPorfolio\Assets\crystal_cluster_asset\Emerald_Crystal_Rock_Cluster_preview.png"
importFile inputPath #noPrompt
lookTarget=point name:"LookTarget" pos:[0,0,1.45] size:0.1
cam=targetCamera name:"PreviewCamera" pos:[5.8,-8.5,4.2] target:lookTarget
cam.fov=45
light1=omniLight name:"Key" pos:[3,-4,7]; light1.multiplier=2.0
light2=omniLight name:"Fill" pos:[-4,-2,4]; light2.multiplier=0.8
light3=omniLight name:"Rim" pos:[0,4,5]; light3.multiplier=1.3
renderWidth=512; renderHeight=512
render camera:cam outputfile:renderPath vfb:false
format "PREVIEW=%\n" renderPath
