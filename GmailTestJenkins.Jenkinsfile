pipeline {
    agent any
    
    options {
        timestamps()
    }
    
    stages {
        stage ('Checkout Code') {
            steps {
                script {
                    git 'https://github.com/beyfound/GmailTest.git'
                }
            }
        }

        stage ('Build Project') {
            steps {
                script {
                    echo "${env.BUILD_NUMBER}"
                    bat "\"${tool 'MsBuild'}\" GmailTest.sln /p:Configuration=Release /p:Platform=\"Any CPU\" /p:ProductVersion=1.0.0.${env.BUILD_NUMBER}"
                }
            }
                    
        }


        stage ('Executing to send email') {
             steps {
                script {
                    echo "Executing  to send email with -send_addrss ${params.SENDER_ADDRESS} -sender_password ${params.SENDER_PASSWORD} -receiver_address ${params.RECEIVER_ADDRESS} -email_ubject ${params.EMAIL_SUBJECT} -email_body ${params.EMAIL_BODY}"
                    bat ".\\bin\\Release\\GmailTest.exe ${params.SENDER_ADDRESS} ${params.SENDER_PASSWORD} ${params.RECEIVER_ADDRESS} \"${params.EMAIL_SUBJECT}\" \"${params.EMAIL_BODY}\""
                }
             }
        }
        
         stage('Done') {
            steps {
                script {
                    echo "DONE"
                }
            }
        }
    }
}