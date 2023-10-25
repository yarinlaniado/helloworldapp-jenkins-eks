pipeline {
    agent {
        kubernetes {
            yaml '''
apiVersion: v1
kind: Pod
metadata:
  name: dotnet-app
  namespace: development
  labels:
    app: dotnet-app
spec:
  containers:
  - name: build
    image: mcr.microsoft.com/dotnet/sdk:7.0-alpine
    command:
    - /bin/sh
    - -c
    - 'sleep infinity'
  - name: dind
    image: docker:18.05-dind
    securityContext:
      privileged: true
  - name: ssh-agent
    image: linuxserver/openssh-server:latest
    ports:
    - containerPort: 22      
  volumes:
  - name: dind-storage
    emptyDir: {}  # This volume definition is now at the Pod level
  volumeMounts:
  - name: dind-storage
    mountPath: /var/lib/docker

     
'''
            defaultContainer 'build'
        }       
    }
    environment {
      VERSION = "1.0.${env.BUILD_ID}"
      remoteCommands =
      """kubectl get nodes """
    }
    stages {
        stage('Build') {
            steps {
                container('build') {
                    sh 'cd webapp/HelloWorldApp && dotnet publish -c Release -o out'
                    
                }
            }
        }
        stage('Push to Docker Hub') {
            steps {
                container('dind')  {
                        withCredentials([usernamePassword(credentialsId: 'DOCKERHUB_PW', passwordVariable: 'DOCKER_PASSWORD', usernameVariable: 'DOCKER_USERNAME')]) {
                            sh 'docker login -u $DOCKER_USERNAME -p $DOCKER_PASSWORD'                   
                            sh "docker build -t yarinlaniado/helloworld-webapp ./webapp/HelloWorldApp"
                            sh "docker build -t yarinlaniado/helloworld-webapp:${VERSION} ./webapp/HelloWorldApp"                            
                            sh "docker push yarinlaniado/helloworld-webapp:${VERSION}"
                            sh "docker push yarinlaniado/helloworld-webapp"                       
                        }

                }
            }
        }
          stage('deploy') {
            steps {
              container('ssh-agent') {
               sshagent(['DEPLOY_AGNET_SSH']) {
                sh 'ssh -o StrictHostKeyChecking=no -tt ubuntu@ec2-35-180-140-207.eu-west-3.compute.amazonaws.com $remoteCommands'
              }
              }

            }
        }

    }
}

