pipeline {
    agent any
    
	parameters {
		choice( name :'ACTION_TYPE', choices: ['Create', 'Reply', 'Find'], description: 'the build action type')
		string( name : 'SENDER_ADDRESS', defaultValue: "st.customer01@gmail.com", description:'')
		string( name : 'ADDRESS_PASSWORD', defaultValue: "Forgerock1", description:'')
		string( name : 'SENDER_DISPLAY', defaultValue: "st.customer", description:'')
		string( name : 'RECEIVER_ADDRESS', defaultValue: "dopicokoga@gmail.com", description:'')
		string( name : 'EMAIL_SUBJECT', defaultValue: "Hello", description:'')
		string( name : 'EMAIL_BODY', defaultValue: "hello, how are you", description:'')
	}
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
                    echo "Executing  to send email with -action_type ${ACTION_TYPE} -send_addrss ${SENDER_ADDRESS} -display_name ${SENDER_DISPLAY} -sender_password ${ADDRESS_PASSWORD} -receiver_address ${RECEIVER_ADDRESS} -email_ubject ${EMAIL_SUBJECT} -email_body ${params.EMAIL_BODY}"
                    bat ".\\bin\\Release\\GmailTest.exe \"${ACTION_TYPE}\" \"${SENDER_ADDRESS}\" \"${ADDRESS_PASSWORD}\" \"${SENDER_DISPLAY}\" \"${RECEIVER_ADDRESS}\" \"${EMAIL_SUBJECT}\" \"${EMAIL_BODY}\" "
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