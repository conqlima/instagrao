# Instagrao
Solução apresentado para o desafio proposto pela Solvimm.

* Função ExtractMetadata, que é é chamada quando um novo arquivo é carregado no S3. Ela
deverá extrair os metadados da imagem (dimensões, tamanho do arquivo) e armazenar no
DynamoDB.
* Função GetMetadata, que recebe a requisição de um endpoint criado pelo AWS API Gateway.
Ela irá receber o parâmetro s3objectkey e retornar os metadados armazenados no DynamoDB.
* Função GetImage, que recebe como parâmetro o s3objectkey e faz o download da imagem.
* Função InfoImages, que não recebe nenhum parâmetro e pesquisa os metadados salvos no
DynamoDB para retornar as seguintes informações:
  * Qual é a imagem que contém o maior tamanho?
  * Qual é a imagem que contém o menor tamanho?
  * Quais os tipos de imagem salvas no S3?
  * Qual a quantidade de cada tipo de imagem salva?
