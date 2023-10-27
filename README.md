# Jenkins CI/CD Pipeline for .NET Application on AWS EKS

My mission was to create a Kubernetes cluster with a Jenkins pod and a pipeline that runs a build in one namespace and runs the deployment in another namespace.

## Table of Contents

- [Introduction](#introduction)
- [Infrastructure](#infrastructure)
- [Setup](#setup)
- [Jenkins Configuration](#jenkins-configuration)
- [Pipeline](#pipeline)
- [Usage](#usage)

## Introduction

This project is focused on setting up and automating a Jenkins pipeline for compiling a .NET-based Web application and deploying it to a Kubernetes cluster. It covers the creation of infrastructure on AWS using EKS, the configuration of Jenkins, and the implementation of a CI/CD pipeline.

## Infrastructure

### VPC

I created a VPC for the EKS cluster with the following components:

- Availability Zones (AZs)
- Security Groups
- Subnets for each AZ, public and private

### EKS Cluster

I created an Amazon Elastic Kubernetes Service (EKS) cluster that uses the created VPC with the following components:

- 2 node groups
- EBS-CSI for cloud persistence storage
- EFS-CSI, which created 2 Kubernetes objects for EFS usage:
  - `efs csi node`
  - `csi efs driver`

### Network Load Balancer (NLB) and Ingress

To manage traffic, I set up:
- An NLB provisioned on AWS
- Two Ingress resources:
  - One for the application - [http://laniado.webapp.io](http://laniado.webapp.io)
  - One for Jenkins - [http://laniado.jenkins.io](http://laniado.jenkins.io)

I also simulated DNS resolution by performing an nslookup to the nlb address to obtain an IP and added it to `/etc/hosts` for local testing.

### Amazon EFS (Elastic File System)

For data persistence, I created the following AWS objects:

- Security Group for EFS, opening port 2049 for NFS
- Storage Class on the EKS
- Persistent Volume (PV) with EFS size configuration on the EKS
- Persistent Volume Claim (PVC) to claim the PV for Jenkins on the EKS
- Mount Targets, using the EFS ID, security group, and each node's IP
- Access Point, specifying mount targets, root directory, and POSIX permissions

## Setup

### Initial Setup

- I used Terraform to create the EKS cluster on AWS.
- Amazon EFS was set up to ensure data availability for the Jenkins server even if a node fails.
- An agent was created and equipped with necessary tools such as kubectl, AWS CLI, and an SSH key.

After the EKS is provisioned, I did the following:
- For NLB and Ingress, I used YAML files to configure them and applied them to the cluster.
- I created 2 namespaces: devops, deployment.
- I set up a simple service for the web app and an ingress for it.
- I created a simple deployment that uses the initial version of the web app.
- For the Jenkins server, I used HELM to install Jenkins in the 'devops' namespace.

## Jenkins Configuration

- Helm was used to deploy Jenkins to the EKS cluster with specific configurations in the 'devops' namespace.
- Jenkins plugins were installed, including SSH agent, stage view, k8s, git, Workspace Cleanup, and recommended plugins.
- Several credentials were created:
  - `DOCKERHUB_PW`: Docker Hub username and password for future project delivery.
  - `DEPLOY_AGENT_SSH`: SSH secret for the agent that runs and deploys the application.
  
I created a job called hello-world-k8s.
I configured my Kubernetes cloud in the Jenkins controller to use the local EKS Kubernetes API, and by default, it will use the namespace called 'devops.' For that, Helm automatically created all the thing that are necessary for it to work.  

## Pipeline

The pipeline is using Kubernetes ephemeral agents and runs them on devops namespace:

- "build" - dotnet/sdk:7.0-alpine image, which is the default image for building the project.
- "dind" - docker:18.05-dind image used to create the image, tag it, and push it to my DockerHub registry repo called 'yarinlaniado/helloworld-webapp.'
- "ssh-agent" - linuxserver/openssh-server:latest image used to SSH to the agent and send the rolling update of the new image to the deployment.

The Jenkins pipeline follows a four-stage process:

### SCM Checkout

- Retrieves the source code from the GitHub repository, including Jenkinsfile and web app for further build.

### Build

- Compiles the .NET application using the .NET SDK image in a container.

### Push to Docker Hub

- Using the dind container to build and push the Docker images.
- Builds Docker images from the application and pushes them to Docker Hub.
- Uses Jenkins credentials (`DOCKERHUB_PW`) for Docker Hub login.
- Two images are created:
  - `yarinlaniado/helloworld-webapp:${VERSION}` (with the build ID)
  - `yarinlaniado/helloworld-webapp:latest`

### Deploy

- Using the ssh-agent container to SSH to the agent.
- Uses SSH to connect to a remote server and updates a Kubernetes deployment to use the newly built Docker image.
- Sets the image to the new image that was pushed before.
- Initiates a rolling update for the new image.

### POST - always

- using Workspace Cleanup plugin to delete the working directory 

## Usage

After the pipeline is complete, the application is accessible via the Ingress setup:
- [laniado.webapp.io](http://laniado.Ibapp.io)

You can verify that the image has been changed to the latest image by describing the pod and checking the site.
