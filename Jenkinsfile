pipeline {
  agent any
  tools {
    dotnetsdk 'SDK 8'
  }
  environment {
      DOTNET_SYSTEM_GLOBALIZATION_INVARIANT = "true"
      PATH = "${env.HOME}/.dotnet/tools:${env.PATH}" // Voeg ~/.dotnet/tools toe aan het PATH
      DOCKER_IMAGE = "casgoorman/orderservice" // Pas aan naar jouw image
      DOCKER_TAG = "latest" // Of gebruik bijv. BUILD_NUMBER voor een unieke tag
  }
  stages{
    stage('Clean and checkout'){
      steps{
        cleanWs()
        checkout scm
      }
    }
      
    stage('Restore'){
      steps{
        sh 'dotnet restore OrderService.sln'
      }
    }
      
    stage('Clean'){
      steps{
        sh 'dotnet clean OrderService.sln'
      }
    }
      
    stage('Build'){
      steps{
        sh 'dotnet build OrderService.sln --configuration Release'
      }
    }
    
    stage('Install SonarScanner') {
      steps {
          sh 'dotnet tool install --global dotnet-sonarscanner'
          sh 'export PATH="$PATH:~/.dotnet/tools"' // Zorg ervoor dat de tool in het PATH staat
        }
    }
    stage('SonarQube Analysis') {
        steps {
          withSonarQubeEnv('SonarQube') { // Naam van de SonarQube server zoals ingesteld in Jenkins
            sh '''
            dotnet sonarscanner begin /k:"OrderService"
            dotnet build --configuration Release
            dotnet sonarscanner end
            '''
        }
      }
    }
    //stage("Quality Gate") {
    //  steps {
    //   timeout(time: 1, unit: 'MINUTES') {
    //        waitForQualityGate abortPipeline: true
    //     }
    //   }
    // }
    stage('Build Docker Image') {
        steps {
          dir('OrderService') {
            script {
                // Zorg dat je een Dockerfile in je project hebt
                sh """
                docker build -t ${env.DOCKER_IMAGE}:${env.DOCKER_TAG} .
                """
            }
          }
        }
    }
    stage('Push Docker Image') {
        steps {
            script {
                // Login naar Docker Registry (indien nodig)
                withDockerRegistry([credentialsId: 'docker-hub-credentials', url: 'https://index.docker.io/v1/']) {
                    sh """
                    docker push ${env.DOCKER_IMAGE}:${env.DOCKER_TAG}
                    """
                }
            }
        }
    }
    
  } 
}
