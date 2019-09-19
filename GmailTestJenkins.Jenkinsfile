pipeline {
    agent any
    
    options {
        timeout(1)
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
            when { changeset '*'}
            steps {
                script {
                    bat ".\\Nuget\\nuget.exe restore GmailTest.sln"
                    echo "${env.BUILD_NUMBER}"
                    bat "\"${tool 'MSBuild'}\" GmailTest.sln /p:Configuration=Release /p:Platform=\"Any CPU\" /p:ProductVersion=1.0.0.${env.BUILD_NUMBER}"
                }
            }
                    
        }


        stage ('Executing to send email') {
             steps {
                script {
                    echo "Executing  to send email with -action_type ${params.ACTION_TYPE} -send_addrss ${params.SENDER_ADDRESS} -display_name ${params.SENDER_DISPLAY} -sender_password ${params.ADDRESS_PASSWORD} -receiver_address ${params.RECEIVER_ADDRESS} -email_ubject ${params.EMAIL_SUBJECT} -email_body ${params.EMAIL_BODY}"
                    bat ".\\bin\\Release\\GmailTest.exe \"${params.ACTION_TYPE}\" \"${params.SENDER_ADDRESS}\" \"${params.ADDRESS_PASSWORD}\" \"${params.SENDER_DISPLAY}\" \"${params.RECEIVER_ADDRESS}\" \"${params.EMAIL_SUBJECT}\" \"${params.EMAIL_BODY}\" "
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