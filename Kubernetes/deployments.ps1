kubectl create namespace azurelabs-acoes

kubectl apply -f .\acoes-secrets.yaml -n azurelabs-acoes

kubectl describe secret acoes-secrets -n azurelabs-acoes

kubectl create -f .\worker-acoes-deployment.yaml -n azurelabs-acoes

kubectl get deployments -n azurelabs-acoes

kubectl get pods -n azurelabs-acoes

kubectl create -f .\api-acoes-deployment.yaml -n azurelabs-acoesbernetes

kubectl create -f .\api-acoes-service.yaml -n azurelabs-acoesbernetes

kubectl get deployments -n azurelabs-acoes

kubectl get pods -n azurelabs-acoes

kubectl get services -n azurelabs-acoes