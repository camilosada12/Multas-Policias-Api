pipeline {
    agent any

    environment {
        DOTNET_CLI_HOME = 'C:\\jenkins\\.dotnet'
        DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
        DOTNET_NOLOGO = '1'
    }

    stages {

        stage('Leer entorno desde .env') {
            steps {
                script {
                    // lee el valor de ENVIRONMENT desde el archivo .env en la raíz del proyecto
                    def envValue = powershell(
                        script: "(Get-Content .env | Where-Object { \$_ -match '^ENVIRONMENT=' }) -replace '^ENVIRONMENT=', ''",
                        returnStdout: true
                    ).trim()

                    if (!envValue) {
                        error "❌ No se encontró ENVIRONMENT en el archivo .env raíz"
                    }

                    env.ENVIRONMENT = envValue.toLowerCase()
                    env.ENV_DIR = "Back/environments/${env.ENVIRONMENT}"
                    env.COMPOSE_FILE = "${env.ENV_DIR}/docker-compose.override.yml"
                    env.ENV_FILE = "${env.ENV_DIR}/.env"

                    echo "✅ Entorno detectado: ${env.ENVIRONMENT}"
                    echo "📄 docker-compose: ${env.COMPOSE_FILE}"
                    echo "📁 archivo .env: ${env.ENV_FILE}"
                }
            }
        }

        stage('Restaurar dependencias .NET') {
            steps {
                dir('Back') {
                    bat '''
                        echo 🧩 Restaurando dependencias .NET...
                        dotnet restore
                    '''
                }
            }
        }

        stage('Compilar proyecto .NET') {
            steps {
                dir('Back') {
                    echo '⚙️ Compilando proyecto Multas-Policias-Api...'
                    bat 'dotnet build --configuration Release'
                }
            }
        }

        stage('Publicar y construir imagen Docker') {
            steps {
                dir('Back') {
                    echo "🐳 Construyendo imagen Docker (${env.ENVIRONMENT})..."
                    bat "docker build -t multas-api-${env.ENVIRONMENT}:latest -f Dockerfile ."
                }
            }
        }

        stage('Desplegar API con Docker Compose') {
            steps {
                dir('Back') {
                    echo "🚀 Desplegando API en entorno: ${env.ENVIRONMENT}"
                    bat """
                        docker compose -f ${env.COMPOSE_FILE} --env-file ${env.ENV_FILE} down || exit /b 0
                        docker compose -f ${env.COMPOSE_FILE} --env-file ${env.ENV_FILE} up -d --build
                    """
                }
            }
        }

        stage('Verificar estado de contenedores') {
            steps {
                bat 'docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"'
            }
        }
    }

    post {
        success {
            echo "🎉 Despliegue completado correctamente para ${env.ENVIRONMENT}"
        }
        failure {
            echo "💥 Error durante el despliegue en ${env.ENVIRONMENT}"
        }
    }
}
