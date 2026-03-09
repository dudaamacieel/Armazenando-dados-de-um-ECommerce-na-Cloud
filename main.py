import streamlit as st
from azure.storage.blob import BlobServiceClient
import pyodbc
import os
import uuid
from dotenv import load_dotenv

load_dotenv()



blobConnectionString = os.getenv('BLOB_CONNECTION_STRING')
blobContainerName = os.getenv('BLOB_CONTAINER_NAME')
blobAccountName = os.getenv('BLOB_ACCOUNT_NAME')

SQL_SERVER = os.getenv('SQL_SERVER')
SQL_DATABASE = os.getenv('SQL_DATABASE')
SQL_USER = os.getenv('SQL_USER')
SQL_PASSWORD = os.getenv('SQL_PASSWORD')

st.title('Cadastro de Produtos')



def get_connection():
    return pyodbc.connect(
        f"DRIVER={{ODBC Driver 17 for SQL Server}};"
        f"SERVER={SQL_SERVER};"
        f"DATABASE={SQL_DATABASE};"
        f"UID={SQL_USER};"
        f"PWD={SQL_PASSWORD};"
        "Encrypt=yes;"
        "TrustServerCertificate=no;"
        "Connection Timeout=30;"
    )



def upload_blob(file):
    blob_service_client = BlobServiceClient.from_connection_string(blobConnectionString)
    blob_name = str(uuid.uuid4()) + "_" + file.name
    blob_client = blob_service_client.get_blob_client(container=blobContainerName, blob=blob_name)
    blob_client.upload_blob(file.read(), overwrite=True)

    return f"https://{blobAccountName}.blob.core.windows.net/{blobContainerName}/{blob_name}"



def insert_product(nome, preco, descricao, imagem):
    try:
        image_url = upload_blob(imagem)

        conn = get_connection()
        cursor = conn.cursor()

        cursor.execute(
            "INSERT INTO Produtos (nome, preco, descricao, imagem_url) VALUES (?, ?, ?, ?)",
            (nome, preco, descricao, image_url)
        )

        conn.commit()
        conn.close()

        return True

    except Exception as e:
        st.error(f'Erro ao inserir produto: {e}')
        return False



def list_products():
    try:
        conn = get_connection()
        cursor = conn.cursor()

        cursor.execute("SELECT id, nome, descricao, preco, imagem_url FROM Produtos")
        produtos = cursor.fetchall()

        conn.close()
        return produtos

    except Exception as e:
        st.error(f'Erro ao listar produtos: {e}')
        return []



def delete_product(product_id):
    try:
        conn = get_connection()
        cursor = conn.cursor()

        cursor.execute("DELETE FROM Produtos WHERE id = ?", (product_id,))
        conn.commit()
        conn.close()

    except Exception as e:
        st.error(f'Erro ao excluir produto: {e}')



product_name = st.text_input('Nome do Produto')
product_price = st.number_input('Preço do Produto', min_value=0.0, format="%.2f")
product_description = st.text_area('Descrição do Produto')
product_image = st.file_uploader('Imagem do Produto', type=['jpg', 'png', 'jpeg'])

if st.button('Salvar Produto'):
    if product_name and product_description and product_image:
        if insert_product(product_name, product_price, product_description, product_image):
            st.success('Produto salvo com sucesso!')
            st.rerun()
    else:
        st.error('Preencha todos os campos.')


st.header('Produtos Cadastrados')

produtos = list_products()
colunas = st.columns(3)

for i, produto in enumerate(produtos):
    with colunas[i % 3]:
        with st.container(border=True):

            product_id = produto[0]
            nome = produto[1]
            descricao = produto[2]
            preco = produto[3]
            imagem = produto[4]

            st.subheader(nome)
            st.write(f"R$ {preco:.2f}")
            st.write(descricao)
            st.image(imagem, width=200)

            if st.button("Excluir", key=f"delete_{product_id}"):
                delete_product(product_id)
                st.rerun()