## Extract surface command

Notas: 
- El comando ExtractTINSurfacesToDWG exporta varias superficies recortadas a partir de una superficie original en formato DWG
- El comando ExtractTINSurfacesToXML exporta varias superficies recortadas a partir de una superficie original en formato XML
- El comando ExportTINSurfaceToXML exporta una superficie recortada a partir de una superficie original en formato XML


## Border Updated

Original TinSample, cyan rectangle es el borde usado para el crop<br>
<img src="/img/_original_tin.png" alt="Original TIN" width="500">


Exported Crop Tin Resultante <br>
<img src="/img/crop_tin.png" alt="Cropped" width="500">


>[!WARNING]
Exported crop resultante fallara cuando el contorno genera más de un border en la superficie resulante.
><img src="/img/fail_tin.png" alt="Fail" width="500">


