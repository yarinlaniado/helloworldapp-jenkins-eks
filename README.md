

# Jenkins CI/CD Pipeline for .NET Application on AWS EKS

My mission was to create k8s cluster with a jenkins pod and a pipeline that runs a build
in one namespace and runs the deployment on another namespcae
## Table of Contents

- [Introduction](#introduction)
- [Infrastructure](#infrastructure)
- [Setup](#setup)
- [Jenkins Configuration](#jenkins-configuration)
- [Pipeline](#pipeline)
- [Usage](#usage)
## Introduction

This project is focused on the setup and automation of a Jenkins pipeline for compiling a .NET-based Ib application and deploying it to a Kubernetes cluster. It covers the creation of infrastructure on AWS using EKS, configuration of Jenkins, and the implementation of a CI/CD pipeline.


## Infrastructure

### VPC

I created a VPC for EKS cluster and its with the following components:

- Availability Zones (AZs)
- Security Groups
- subnets for each AZ, public and private

### EKS Cluster

I created an Amazon Elastic Kubernetes Service (EKS) cluster that uses the created vpc with the following components:

- 2 node groups
- EBS-CSI for cloud persistence storage
- EFS-CSI created 2 k8s objects for EFS usage : 
  - efs csi node
  - csi efs driver


### Network Load Balancer (NLB) and Ingress

To manage traffic, I set up:
- An NLB provisioned on AWS
- Two Ingress resources:
  - One for the application - http://laniado.webapp.io
  - One for Jenkins - http://laniado.jenkins.io

I also simulated DNS resolution by doing an nslookup to get an IP and added it to /etc/hosts for local testing.

### Amazon EFS (Elastic File System)

For data persistence, I created the following AWS objects:


- Security Group for EFS on port 2049
- Storage Class on the EKS
- Persistent Volume (PV) with EFS size configuration on the EKS
- Persistent Volume Claim (PVC) to claim the PV for Jenkins on the EKS
- Mount Targets, using the EFS ID, security group, and each node's IP
- Access Point, specifying mount targets, root directory, and POSIX permissions

## Setup

### Initial Setup <-

- I used Terraform to create the EKS cluster on AWS.
- Amazon EFS was set up to ensure data availability of the jenkins server even if a node fails.
- An agent was created and equipped with necessary tools such as kubectl, AWS CLI, and an SSH key.

After the EKS is provisioned I did this:
- For NLB and Ingress, I used YAML files to configure them, and applying them on the cluster.
- 3 namespaces - devops development deployment
- a simple service for the webapp and a ingress for it as well
- a simple deployment that uses initial version of the web app
- For the jenkins server I used HELM to install the jenkins on the namespace called devops

## Jenkins Configuration

- Helm was used to deploy Jenkins to the EKS cluster with specific configurations on devops namespace

- Jenkins plugins were installed, including SSH agent, stage view, k8s, git, and recommended plugins.

- Several credentials were created:
  - `DOCKERHUB_PW`: Docker Hub username and password for future project delivery.
  - `K8S_NS_DEPLOYMENT`: Kubernetes namespace development secret with the service account token for creating ephemeral agents.
  - `DEPLOY_AGNET_SSH`: SSH secret for the agent that runs and deploys the application.

  I created a job called hello-world-k8s

  I configured my kubernetes cloud in the jenkins controller that it will use the local eks k8s api and by deafult will use the namespace called development
  for that, I needed to create a secret and a service account in order to have control over that namespace

## Pipeline

The pipeline is using k8s ephemeral agents

- “build” - dotnet/sdk:7.0-alpine image that is the default image and upon it I build the projcet
-  “dind” - docker:18.05-dind image that is used to create the image, tag it and push to my DockerHub registry repo called yarinlaniado/helloworld-webapp
- “ssh-agent” - linuxserver/openssh-server:latest image that used to ssh to the agent and send the rolling update of the new image to the deployment


The Jenkins pipeline follows a four-stage process:

### SCM Checkout

- Retrieves the source code from the GitHub repository.
  - including Jenkinsfile and webapp for futher build.

### Build

- Compiles the .NET application using the .NET SDK image. using a continer called build

### Push to Docker Hub
- using the dind container to build and push the docker images
- Builds Docker images from the application and pushes them to Docker Hub.
- Uses Jenkins credentials (`DOCKERHUB_PW`) for Docker Hub login.
- Two images are created:
  - `yarinlaniado/helloworld-webapp:${VERSION}` (with the build ID)
  - `yarinlaniado/helloworld-webapp:latest`

### Deploy
- using the ssh-agent container to ssh to the agent
- Uses SSH to connect to a remote server and updates a Kubernetes deployment to use the newly built Docker image.
- Sets the image to the new image that pushed before.
- initiate rolling update for the new image

## Usage

After the pipeline is complete, the application is accessible via the Ingress setup:
- [laniado.webapp.io](http://laniado.Ibapp.io)

By describing the pod and checking the site, you can verify that the image has been changed to the latest image.
