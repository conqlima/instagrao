# Instagrao
Solução apresentado para o desafio proposto pela Solvimm.

* **ExtractMetadata**: é chamada quando um novo arquivo é carregado no S3 e extrai os metadados da imagem tamanho em bytes, largura e altura e armazena na tabela ImageMetadata no DynamoDB.
   * `Entrada`: Evento de um novo arquivo no S3 Bucket chamado 'conqlimabucket'
   * `Saída`: Informações sobre a imagem na tabela 'ImageMetadata' no DynamoDB
   
* **GetMetadata**: Recebe a requisição de um endpoint criado pelo AWS API Gateway.
   * `Entrada`: Requisição http para o path ``/{s3ObjectKey}``
   * `Saída`: 
   ```json
   {
    "ImageMetadataId": "exampleImage.png",
    "Length": "362997",
    "Height": "618",
    "Width": "1932"
   }
  ```

* **GetImage**: Recebe a requisição de um endpoint criado pelo AWS API Gateway. Adicionar o valor ``*/*`` no API Gateway console em *Configurações*, seção *Tipos de mídia binários*
   * `Entrada`: Requisição http para o path ``/{s3ObjectKey}``
   * `Saída`: Download do arquivo (caso seja utilizado o Postman, utilizar a opção 'Send and Download').
   
* **InfoImages**: Não recebe nenhum parâmetro e pesquisa os metadados salvos no DynamoDB para retornar as seguintes informações:
  * Qual é a imagem que contém o maior tamanho?
  * Qual é a imagem que contém o menor tamanho?
  * Quais os tipos de imagem salvas no S3?
  * Qual a quantidade de cada tipo de imagem salva?
  * `Entrada`: Requisição http para o path /
  * `Saída`: 
  ```json
  { 
    "BiggestImage": {     
        "ImageMetadataId": "exampleImage.png", 
        "Length": "362997",
        "Height": "2576",
        "Width": "1932"
    },
    "SmallestImage": {
        "ImageMetadataId": "anotherExampleImage.jpg",
        "Length": "37747",
        "Height": "618",
        "Width": "819"
    },
    "ImageExtensions": [
        {
            "Name": ".png",
            "Quantity": 1
        },
        {
            "Name": ".jpg",
            "Quantity": 1
        }
    ]
  }
```
