#BossFightApp
This is the Boss Fight application.

To run it, you need:
* Docker Desktop with Kubernetes enabled.
* kubectl installed and configured.
* .NET 10 SDK.
* A Docker Hub account.


Step 1: Build and publish the application images.
Open cmd in the application folder.
Log in to Docker Hub:
>docker login
Build and publish the three services, replacing YOUR_USERNAME with your Docker Hub username:
>docker build -f CharacterWebsite/Dockerfile -t YOUR_USERNAME/character-website:1.0 .
>docker build -f CharacterService/Dockerfile -t YOUR_USERNAME/character-service:1.0 .
>docker build -f HistoryService/Dockerfile -t YOUR_USERNAME/historyservice:1.0 .

>docker push YOUR_USERNAME/character-website:1.0
>docker push YOUR_USERNAME/character-service:1.0
>docker push YOUR_USERNAME/historyservice:1.0

Also update the image names in k8s/app.yaml to match your repositories.

2. Configure database credentials
In the k8s folder, create a file called db-secrets.env
Write the following in this file:
characters-db-password=YOURPASSWORD
history-db-password=YOURPASSWORD

...except that you replace YOURPASSWORD with your desired password.

Create the Kubernetes namespace:
>kubectl apply -f k8s/namespace.yaml

Create the database Secret:
>kubectl create secret generic db-secrets --namespace character-app --from-env-file=k8s/db-secrets.env

3. Deploy the application
>kubectl apply -f k8s/app.yaml -n character-app

Forward the port to your machine:
>kubectl port-forward -n character-app svc/website 8080:8080

Open this in a browser:
http://localhost:8080/

Now the web application should be available to you!