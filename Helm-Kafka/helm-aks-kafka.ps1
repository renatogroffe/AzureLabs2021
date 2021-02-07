kubectl create namespace azurelabs-kafka

helm repo add bitnami https://charts.bitnami.com/bitnami

helm repo update

helm repo list

helm install k8s-kafka --set externalAccess.enabled=true,externalAccess.service.type=LoadBalancer,externalAccess.service.port=19092,externalAccess.autoDiscovery.enabled=true,serviceAccount.create=true,rbac.create=true bitnami/kafka -n azurelabs-kafka