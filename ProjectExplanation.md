# Project README

This README provides an overview of the steps and processes involved in setting up a Jenkins pipeline for compiling and deploying a .NET-based web application in a Kubernetes (k8s) cluster. The project involves creating a Jenkins instance inside a pod in the k8s cluster in devops namespace, connecting it to Git, and configuring the pipeline to compile  the web application on devops namespace, and deploy it on deployment namespace

## Project Steps

### Web Application (WEB-APP)

The web application is a simple "Hello World" application built with C# and ASP.NET. It features a single route that returns a "Hello World" message.

### Amazon Elastic Kubernetes Service (EKS)

1. **Infrastructure Setup**: Before setting up Jenkins, the infrastructure was established for the EKS cluster, including a VPC with security groups (SGs), Availability Zones (AZs), and subnets for each AZ.

2. **EKS Cluster Creation**: The EKS cluster was created using Terraform, with two node groups and EBS-CSI for cloud persistence storage, including EFS CSI drivers.

3. **Network Load Balancer (NLB)**: An NLB was created using a Nginx controller YAML file, providing an external entry point to Jenknis and later on, to the application.

4. **Ingress Configuration**: To expose the applications, two ingresses were created, one for the web app and one for Jenkins, for local testing I took the ip of the nlb dns and wrote it inside /etc/hosts

### Amazon Elastic File System (EFS)

1. **EFS Configuration**: EFS was used to provide persistence storage for Jenkins.

2. **AWS Objects Setup**: The following AWS objects were created for EFS: Security Group (SG) for EFS (port 2049), Storage Class Kubernetes object, Persistent Volume (PV) for EFS, Persistent Volume Claim (PVC) for Jenkins, Mount Target (EFS ID, SG, and node IPs), and Access Point (mount target, root directory, and posix permissions).

### Jenkins Setup

1. **Helm Installation**: Jenkins was installed in the cluster using Helm. The existing VPC, created earlier was used to enable persistence storage while disabling Jenkins Configuration as Code to prevent configuration resets.
It is installing it on namespace called devops
```sheel
controller.JCasC.defaultConfig=false - to make the configuration not resetting to the default
persistence.existingClaim=efs-claim - make jenkins use the persistence storage that I created for it
```
Helm command
```shell
helm install jenkins jenkins/jenkins --set rbac.create=true,controller.servicePort=80,persistence.existingClaim=efs-claim,controller.JCasC.defaultConfig=false -n devops
```

2. **Plugin Installation**: Essential plugins like SSH agent, stage view, Kubernetes, and Git, along with recommended plugins, were downloaded.

3. **Credentials Setup**: The following credentials were created:
   - `DOCKERHUB_PW`: Dockerhub username and password for project delivery.
   - `DEPLOY_AGENT_SSH`: SSH secret for the agent responsible for running and deploying the application.

4. **Jenkinsfile Configuration**: A Jenkinsfile was used in the repository to define the pipeline.

### Jenkins Job Configurations

1. **Pipeline Configuration**: The pipeline was set up to pull code from the Git repository and included all necessary files and the Jenkinsfile.

2. **Pipeline Stages**: The pipeline consists of four stages:
   - Declarative: Checkout stage to retrieve the main branch of the repository.
   - Agent: Kubernetes was chosen as the agent, and three containers were created as ephemeral agents for the pipeline: "build" (dotnet/sdk image for building the project), "dind" (docker image for building and pushing Docker images), and "ssh-agent" (linuxserver/openssh-server image for SSH access to the deployment agent).

### Agent

An agent was set up with the necessary tools to interact with the Kubernetes API, including kubectl and AWS CLI. The agent facilitates rolling updates in the Kubernetes cluster.

### Jenkins Pipeline

The pipeline includes the following stages:
1. **Checkout**: Jenkins checks out the Git repository.
2. **Build**: The application is built using the .NET SDK image and prepared for creating a Docker image.
3. **Push to Docker Hub**: The Docker image is built and pushed to Docker Hub, using the credentials from Jenkins credentials (DOCKERHUB_PW).
4. **Deploy**: The application is deployed to the Kubernetes cluster using SSH to connect to a remote server and update a Kubernetes deployment to use the newly built Docker image.
5. **Post-Pipeline Cleanup**: After the pipeline is complete, the working directory is cleaned.

![alt text](https://i.ibb.co/4s3MSyj/Screenshot-from-2023-10-26-15-52-45.png)

Upon successful execution of the pipeline, the web application can be accessed through the previously configured ingress.

**Web Application URL**: http://laniado.webapp.io

By describing the pod and observing changes on the site, you can confirm that the latest image has been deployed.
