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
kubeconfig(caCertificate: 'LS0tLS1CRUdJTiBDRVJUSUZJQ0FURS0tLS0tCk1JSURCVENDQWUyZ0F3SUJBZ0lJVEdCeFBXT3JxeHd3RFFZSktvWklodmNOQVFFTEJRQXdGVEVUTUJFR0ExVUUKQXhNS2EzVmlaWEp1WlhSbGN6QWVGdzB5TXpFd01qVXdPRE0xTXpaYUZ3MHpNekV3TWpJd09EUXdNelphTUJVeApFekFSQmdOVkJBTVRDbXQxWW1WeWJtVjBaWE13Z2dFaU1BMEdDU3FHU0liM0RRRUJBUVVBQTRJQkR3QXdnZ0VLCkFvSUJBUURnWjZWZHZhdjk0eUlaSlJDdzJONWhpR1FySHNLd3RnYWRuWld1Yy92S3JGc01YNGlmWUNZRTF3ZHkKKy8rQ0NrSDd3MGgvZUtWYkQ1RHhUNktuQ0dnWnFSZzhGVW9WMEE1TmxSQ25lOTNuOXM4OWJQVmdxdmhsU0FjVgpGQTlMU0kraHRmRUJka3BsckVNYW1LdjM3ZXhSbld4ZHFzSURDZDdCMFZXVEJmODBhRkgrWTRWUTVjVUgzNlJJCmtKaStwNmQ2S3gzQkNteXI3UDJLYXRxMElzWS8xa0ozZ1p6L1RkT2FhRzRhb0RqTExxYnpIU2R5Qm44RGtpT3UKVS9iZ3dJNThaVllSREVsMWQ1Sm9FOC9uM1NLeEJVcHMzMEo2WFhIVC9vQWtaam5vYUpKaGFIZTdhWk9OWjNLNQpLUllYeUJyV0lKSkNEcExyN01YMXlYVmdhMmRIQWdNQkFBR2pXVEJYTUE0R0ExVWREd0VCL3dRRUF3SUNwREFQCkJnTlZIUk1CQWY4RUJUQURBUUgvTUIwR0ExVWREZ1FXQkJTM1JweDZkd2ZveXAwM1lPN1RCMkVoZVNac2R6QVYKQmdOVkhSRUVEakFNZ2dwcmRXSmxjbTVsZEdWek1BMEdDU3FHU0liM0RRRUJDd1VBQTRJQkFRQmRYbFFjcitLMQptdWpSS0FRa01QbUdDdXJxcERYeWFkTldjSWlBbzNydlhrT0h0Qmt0a1dTaVJaZzhNL1FJT1A4ak82VTZmbnQ2CkthcVp6Z2laRVI3bS8wWENFQjNuMWQ4U2ZCSkJyNmxxUFJzcG4ybEUvenRzeTNVTVNPK0VrVm9lUmNCSmZvcEMKRzFlemlMV2E2YlJTN2ZHNGc0SnQxcEYwUjFvTExOT1NBZmE0RlRwdFZMMlJaVDErMlBXT0Q3QWhhdUNxWDNqVgpFcEFQa3U5SFBScDJYL2FaQ0RKcnlDYTNJNEgwVXl4MU5wNnZQRzBBb214ajZnZzVmWlYwYzJQTC93V1ZyR3E1ClpMMkQ5RXlDQzhjd1ViWmFyWTAyeWNPNEVKYUZZRlMrTTlqWjFXUUorSUFuMlFLQ0s3Q2VpZnFhMDY5QkpDNmsKNGt6QzRpS0JjYSt5Ci0tLS0tRU5EIENFUlRJRklDQVRFLS0tLS0K', credentialsId: 'K8S_NS_DEPLOYMENT', serverUrl: 'https://kubernetes.default') {

						kubectl version

}

            }
        }

    }
}

