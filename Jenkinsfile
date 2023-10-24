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
  - name: docker
    image: docker:dind
    securityContext:
      privileged: true  # Elevates privileges for the container
    restartPolicy: Never    
'''
            defaultContainer 'build'
        }       
    }
    stages {
        stage('Build') {
            steps {
                container('build') {
                    sh 'cd webapp/HelloWorldApp && dotnet build'
                    
                }
            }
        }
        stage('Push to Docker Hub') {
            steps {
                container('docker')  {
                        withCredentials([usernamePassword(credentialsId: 'DOCKERHUB_PW', passwordVariable: 'DOCKER_PASSWORD', usernameVariable: 'DOCKER_USERNAME')]) {
                            sh 'su sudo'
                            sh 'docker login -u $DOCKER_USERNAME -p $DOCKER_PASSWORD'
                            sh "docker build -t yarinlaniado/helloworld-webapp ."
                            sh "docker build -t yarinlaniado/helloworld-webapp:$BUILD_ID ."                            
                            sh "docker push yarinlaniado/helloworld-webapp:$BUILD_ID"
                            sh "docker push yarinlaniado/helloworld-webapp"                            
                        }

                }
            }
        }
    }
}

