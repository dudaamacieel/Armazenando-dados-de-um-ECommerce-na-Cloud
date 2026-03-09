docker build -t blog-eduarda-app:latest .

docker run -d -p 8080:80 blog-eduarda-app:latest

az login

# Create a resource group
az group create --name containerappslab03 --location eastus

# Create Container Registry
az acr create --resource-group containerappslab03 --name bloghenriqueacr --sku Basic

# Login to ACR
az acr login --name bloghenriqueacr

# Tag the image
docker tag blog-eduarda-app:latest bloghenriqueacr.azurecr.io/blog-eduarda-app:latest

# Push the image
docker push bloghenriqueacr.azurecr.io/blog-eduarda-app:latest

#ContainerID: bloghenriqueacr.azurecr.io/blog-eduarda-app:latest
#User: bloghenriqueacr
#Password: 1AL1l3JWpygkCfyYJKsFxk3jqy5qG2JgmZZgsC0T91oZOdPo1vWZJQQJ99CCACYeBjFEqg7NAAACAZCRafdM

# Create Environment container app
az containerapp env create --name blog-eduarda-env --resource-group containerappslab03 --location eastus2

# Create Container App
az containerapp create --name blog-eduarda-app --resource-group containerappslab03 --environment pmaciel-env-001 --image bloghenriqueacr.azurecr.io/blog-eduarda-app:latest --target-port 80 --ingress external --registry-server bloghenriqueacr.azurecr.io
