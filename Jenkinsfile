pipeline {
    agent any

    environment {
        DOTNET_CLI_HOME = 'C:\\jenkins\\.dotnet'
        DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
        DOTNET_NOLOGO = '1'
    }

    stages {

        stage('leer entorno desde .env') {
            steps {
                script {
                    // lee ENVIRONMENT del .env raíz del repo
                    def envValue = powershell(
                        script: "(Get-Content .env | Where-Object { \$_ -match '^ENVIRONMENT=' }) -replace '^ENVIRONMENT=', ''",
                        returnStdout: true
                    ).trim()

                    if (!envValue) {
                        error "❌ no se encontró ENVIRONMENT en el archivo .env raíz"
                    }

                    env.ENVIRONMENT = envValue.toLowerCase()
                    env.ENV_DIR = "Back/environments/${env.ENVIRONMENT}"
                    env.COMPOSE_FILE = "${env.ENV_DIR}/docker-compose.yml"
                    env.ENV_FILE = "${env.ENV_DIR}/.env"

                    echo "✅ entorno detectado: ${env.ENVIRONMENT}"
                    echo "📄 docker-compose: ${env.COMPOSE_FILE}"
                    echo "📁 archivo .env: ${env.ENV_FILE}"
                }
            }
        }

        stage('restaurar dependencias .net 8') {
            steps {
                dir('Back') {
                    bat '''
                        echo 🧩 restaurando dependencias .net 8...
                        dotnet restore
                    '''
                }
            }
        }

        stage('compilar proyecto .net 8') {
            steps {
                dir('Back') {
                    echo '⚙️ compilando proyecto .net 8...'
                    bat 'dotnet build --configuration Release'
                }
            }
        }

        stage('desplegar con docker compose') {
            steps {
                dir('Back') {
                    echo "🚀 desplegando entorno ${env.ENVIRONMENT}..."
                    bat """
                        docker compose -f ${env.COMPOSE_FILE} --env-file ${env.ENV_FILE} down || exit /b 0
                        docker compose -f ${env.COMPOSE_FILE} --env-file ${env.ENV_FILE} up -d --build
                    """
                }
            }
        }

        stage('verificar contenedores activos') {
            steps {
                bat 'docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"'
            }
        }
    }

    post {
        success {
            echo "🎉 despliegue exitoso en ${env.ENVIRONMENT}"
        }
        failure {
            echo "💥 error durante el despliegue en ${env.ENVIRONMENT}"
        }
    }
}
