# IMDb Crawler e RPA

Este é um projeto de **crawler** em **.NET**, que acessa o IMDb, faz login e coleta dados sobre os 20 melhores filmes. Ele utiliza o **Selenium** para fazer o login e **requisições com Csharp** para coletar as informações sobre os filmes, que são então exportadas para um arquivo CSV.

## Passos para Rodar o Projeto

### 1. **Clone o Repositório**

Clone o repositório do projeto para sua máquina local:

```bash
git clone git@github.com:dejazz/crawler_imdb.git
cd crawler_imdb
```
### 2. **Instalar Dependências
Este projeto é baseado no .NET, então você precisa restaurar as dependências e construir o projeto.

Execute os seguintes comandos no terminal dentro da pasta do projeto:

```
dotnet restore
dotnet build
Esses comandos irão restaurar as dependências e compilar o código.
```
### 3. **Configurar Credenciais
Antes de rodar o projeto, você precisa fornecer o e-mail e senha da sua conta no IMDb para que o sistema possa realizar o login e coletar os dados. O código pedir que você insira essas informações no terminal quando você rodar o projeto.

O processo de login ocorre dentro do método LoginAttempts na classe Program que chama a classe LoginRPA feita com Selenium. As credenciais de login serão solicitadas após a execução do programa.

### 4. **Rodar o Projeto
Para rodar o projeto, execute o seguinte comando:

```
dotnet run
```

Ao executar o comando, o sistema solicitar suas credenciais de login (e-mail e senha) no IMDb. Caso o login seja realizado com sucesso, o crawler irá buscar informações sobre os 20 filmes mais populares e salvar os dados no formato CSV.

Exemplo de solicitação de email:

```
Digite as credênciais da sua conta no IMDB
Digite seu email: seu@email.com
```

Exemplo de solicitação de senha:

```
Digite sua senha: suaSenhaAqui
```
Importante: Se o login falhar (por exemplo, se as credenciais estiverem erradas), o sistema irá solicitar que você insira novamente as informações de login.

### 5. **CSV Gerado
Após o processo de extração de dados, o projeto irá criar um arquivo CSV contendo as informações dos filmes coletados. O arquivo será salvo automaticamente na pasta *Downloads* do usuário. O nome do arquivo será algo como *top20filmes_{data_hora_atual}.csv*.

### 6. **O Que Acontece Durante a Execução
O programa começa pedindo o e-mail e senha do usuário para fazer o login no IMDb.
Se as credenciais estiverem corretas, ele obter os cookies necessários para acessar a página do IMDb.
O crawler irá extrair informações como:
Nome do filme
Ano de lançamento
Diretor
Avaliação média
Número de avaliações
Esses dados serão exibidos no console e, em seguida, exportados para um arquivo CSV na pasta Downloads do usuário.

Importante: O Crawler é feito via requisição ao site, utilizando os cookies obtidos no login com o Selenium, junto com um gerenciador de UserAgents para dificultar detecção em sistemas de antibot.

