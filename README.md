# Armazenando dados de um ECommerce na Cloud

## Objetivo:

Projeto desenvolvido durante o curso da DIO com foco na Microsoft Application Platform, simula um Ecommerce utilizando serviços em nuvem da Microsoft Azure.

A aplicação permite cadastrar produtos, armazenar imagens na nuvem e registrar as informações em um banco de dados cloud, demonstrando a integração entre diferentes serviços da plataforma Azure.

## Tecnologias utilizadas:

- Python
- Streamlit
- Azure SQL Database 
- Azure Blob Storage
- PyODBC
- Dotenv

## Funcionamento:

1. O usuário cadastra um produto via interface Streamlit.
2. A imagem é enviada para o Azure Blob Storage.
3. As informações do produto são armazenadas no Azure SQL Database.
4. Os produtos são exibidos dinamicamente na interface. 
5. É possível excluir produtos diretamente da aplicação.

## Como executar:

1. Clone o reposiótio.
2. Crie um ambiente virtual. 
3. Instale as dependências:
    - pip install -r requirements.txt
4. Configure o arquivo .env
5. Execute:
    - streamlit run main.py


## Aprendizados e Insights:

Durante o processo do desenvolvimento deste projeto, foram executados conceitos como:
- Conexão segura com o banco de dados na nuvem.
- Utlização de variáveis de ambiente para segurança.
- Upload e gerenciamento de arquivos no Azure Blob Storage.
- Integração entre serviços cloud.
- Estruturação de aplicações web com Streamlit.

## Print's da Aplicação: Armazenando Dados

Tela inicial:
![alt text](image-1.png)

Produtos listados:
![alt text](image-2.png)

Produto sendo excluído:
![alt text](image-3.png)

Azure SQL:
![alt text](image-4.png)

## Print's da Aplicação: Docker Azure Container App

Tela inicial:
<img width="1365" height="767" alt="image" src="https://github.com/user-attachments/assets/dc703cfb-bfd0-4054-b462-24bef94eede4" />

Produtos listados:
<img width="1365" height="767" alt="image" src="https://github.com/user-attachments/assets/05169e4a-2724-4b2c-81a5-dab274f2437b" />

Azure Resource Group:
<img width="1365" height="767" alt="image" src="https://github.com/user-attachments/assets/d3c5306f-f18a-42a7-b472-92779c527d66" />
