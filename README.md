

# PROJECT - YARIN LANIADO

My mission was to create k8s cluster with a jenkins pod and a pipeline that runs a build
in one namespace and runs the deployment on another
## Table of Contents

- [Introduction](#introduction)
- [Infrastructure](#infrastructure)
- [Setup](#setup)
- [Jenkins Configuration](#jenkins-configuration)
- [Pipeline](#pipeline)
- [Usage](#usage)
## Introduction

This project is focused on the setup and automation of a Jenkins pipeline for compiling a .NET-based web application and deploying it to a Kubernetes cluster. It covers the creation of infrastructure on AWS using EKS, configuration of Jenkins, and the implementation of a CI/CD pipeline.

## Infrastructure

### EKS Cluster

We created an Amazon Elastic Kubernetes Service (EKS) cluster with the following components:
- 2 node groups
- VPC with Availability Zones (AZs)
- Security Groups
- EBS-CSI for cloud persistence storage

### Network Load Balancer (NLB) and Ingress

To manage traffic, we set up:
- An NLB provisioned on AWS
- Two Ingress resources:
  - One for the application - http://laniado.webapp.io
  - One for Jenkins - http://laniado.jenkins.io

We also simulated DNS resolution by doing an nslookup to get an IP and added it to /etc/hosts for local testing.

### Amazon EFS (Elastic File System)

For data persistence, we created the following AWS objects:
- Security Group for EFS on port 2049
- Storage Class
- Persistent Volume (PV) with EFS size configuration
- Persistent Volume Claim (PVC) to claim the PV for Jenkins
- Mount Targets, using the EFS ID, security group, and each node's IP
- Access Point, specifying mount targets, root directory, and POSIX permissions

## Setup

### Initial Setup

- We used Terraform to create the EKS cluster on AWS.

- For NLB and Ingress, we used YAML files to configure them.

- Amazon EFS was set up to ensure data availability even if a node fails.

- An agent was created and equipped with necessary tools such as kubectl, AWS CLI, and an SSH key.

## Jenkins Configuration

- Helm was used to deploy Jenkins to the EKS cluster with specific configurations. 

- Jenkins plugins were installed, including SSH agent, stage view, k8s, git, and recommended plugins.

- Several credentials were created:
  - `DOCKERHUB_PW`: Docker Hub username and password for future project delivery.
  - `K8S_NS_DEPLOYMENT`: Kubernetes namespace development secret with the service account token for creating ephemeral agents.
  - `DEPLOY_AGNET_SSH`: SSH secret for the agent that runs and deploys the application.

## Pipeline

The Jenkins pipeline follows a four-stage process:

### SCM Checkout

- Retrieves the source code from the GitHub repository.

### Build

- Compiles the .NET application using the .NET SDK image.

### Push to Docker Hub

- Builds Docker images from the application and pushes them to Docker Hub.
- Uses Jenkins credentials (`DOCKERHUB_PW`) for Docker Hub login.
- Two images are created:
  - `yarinlaniado/helloworld-webapp:${VERSION}` (with the build ID)
  - `yarinlaniado/helloworld-webapp:latest`

### Deploy

- Deploys the application to a Kubernetes cluster.
- Uses SSH to connect to a remote server and updates a Kubernetes deployment to use the newly built Docker image.
- Sets the image to be the build ID image.

## Usage

After the pipeline is complete, the application is accessible via the Ingress setup:
- [laniado.webapp.io](http://laniado.webapp.io)

By describing the pod and checking the site, you can verify that the image has been changed to the latest image.
