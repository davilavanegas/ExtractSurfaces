## Extract surface command

Notas: 
- El comando ExtractTINSurfacesToDWG exporta varias superficies recortadas a partir de una superficie original en formato DWG
- El comando ExtractTINSurfacesToXML exporta varias superficies recortadas a partir de una superficie original en formato XML
- El comando ExportTINSurfaceToXML exporta una superficie recortada a partir de una superficie original en formato XML


## Border Updated

Original TinSample, cyan rectangle es el borde usado para el crop<br>
![TinOriginal](/img/_original_tin.png)

<img src="/img/_original_tin.png" alt="Original TIN" width="300">


Exported Crop Tin Resultante<br>
![TinCrop](/img/crop_tin.png)

>[!IWARNING]
Exported crop resultante fallara cuando el contorno genera más de un contorno en la superficie resulanto
![TinFail](/img//fail_tin.png)


