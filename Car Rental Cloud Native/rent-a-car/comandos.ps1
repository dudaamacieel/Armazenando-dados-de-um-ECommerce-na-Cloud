az acr login --name acrlab007pmaciel

docker tag bff-rent-car-local acrlab007pmaciel.azurecr.io/bff-rent-car-local:v1

docker push acrlab007pmaciel.azurecr.io/bff-rent-car-local:v1

az containerapp env create --name bff-rent-car-local --resource-group LAB007 --location eastus

--------------------------------------------------------------------------------------------------

{
  "passwords": [
    {
      "name": "password",
      "value": "DgxhmSnXDiIOYZIzZOnbS2jWcqNyYtyadfnBEW4uYIKWB3h1A1rEJQQJ99CCACYeBjFEqg7NAAACAZCRE2Vs"
    },
    {
      "name": "password2",
      "value": "9clNxz4dWmnBI4LNpFAQv56zILgWXlgPHK1uGHe0fV4bWqBU8MUsJQQJ99CCACYeBjFEqg7NAAACAZCRmBHI"
    }
  ],
  "username": "acrLab007pmaciel"
}

----------------------------------------------------------------------------------------------------------
& "C:\Program Files (x86)\Microsoft SDKs\Azure\CLI2\wbin\az.cmd" containerapp create --name bff-rent-car-local --resource-group LAB007 --environment bff-rent-car-local / managedEnvironment-LAB007-b6df --image acrlab007pmaciel.azurecr.io/bff-rent-car-local:v1 --target-port 3001 --ingress external --registry-server acrlab007pmaciel.azurecr.io --registry-username acrLab007pmaciel --registry-password DgxhmSnXDiIOYZIzZOnbS2jWcqNyYtyadfnBEW4uYIKWB3h1A1rEJQQJ99CCACYeBjFEqg7NAAACAZCRE2Vs