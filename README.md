my mission was to create k8s cluster with a jenkins pod and a pipeline that runs a build
in one namespace and runs the deployment on another

Amazon EKS Cluster

Created a vpc to attatch the eks network to it

    Amazon EKS (Elastic Kubernetes Service):
        Provisioned an Amazon EKS cluster using the eks Terraform module.
        Enabled public access to the cluster with a public Elastic IP address.
        Configured the cluster with EKS managed node groups.

    EKS Managed Node Groups:
        Created two EKS managed node groups with specified configurations.
            Instance type: t3a.medium
            Minimum size: 1
            Maximum size: 2

    Amazon EBS Addon (ebs-csi):
        Integrated the EBS CSI driver addon into the EKS cluster. This addon enables interaction with Amazon Elastic Block Store (EBS) to provide persistent storage for applications running in the cluster.
         aws eks update-kubeconfig --region eu-west-3 --name education-eks-ZPz27iKs
       

        
Jenkins:
I used helm to create jenkins master and volumes

webapp 
simple c# web app with a dockerfile that builds the image in one dockerfile and tag the image twice one for a version latest and semantic versioning by the build num
