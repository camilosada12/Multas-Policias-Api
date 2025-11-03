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
                    // lee ENVIRONMENT desde .env en la raíz del repo
                    def envValue = powershell(
                        script: "(Get-Content .env | Where-Object { \$_ -match '^ENVIRONMENT=' }) -replace '^ENVIRONMENT=', ''",
                        returnStdout: true
                    ).trim()

                    if (!envValue) {
                        error "❌ no se encontró ENVIRONMENT en el archivo .env raíz"
                    }

                    env.ENVIRONMENT = envValue.toLowerCase()
                    env.ENV_DIR = "Back/environments/${env.ENVIRONMENT}"
                    env.COMPOSE_FILE = "${env.ENV_DIR}/docker-compose.override.yml"
                    env.ENV_FILE = "${env.ENV_DIR}/.env"

                    echo "✅ entorno detectado: ${env.ENVIRONMENT}"
                    echo "📄 docker-compose: ${env.COMPOSE_FILE}"
                    echo "📁 archivo .env: ${env.ENV_FILE}"
                }
            }
        }

        stage('restaurar dependencias .net') {
            steps {
                dir('Back') {
                    bat '''
                        echo 🧩 restaurando dependencias .net...
                        dotnet restore Web\\Web.csproj
                    '''
                }
            }
        }

        stage('compilar proyecto .net') {
            steps {
                dir('Back') {
                    echo '⚙️ compilando proyecto web (web.csproj)...'
                    bat 'dotnet build Web\\Web.csproj --configuration Release'
                }
            }
        }

        stage('publicar y construir imagen docker') {
            steps {
                dir('Back') {
                    echo "🐳 construyendo imagen docker (multas-api-${env.ENVIRONMENT})..."
                    // construimos con contexto back (donde está Dockerfile)
                    bat "docker build -t multas-api-${env.ENVIRONMENT}:latest -f Dockerfile ."
                }
            }
        }

        stage('desplegar api con docker compose') {
            steps {
                dir('Back') {
                    echo "🚀 desplegando api en entorno: ${env.ENVIRONMENT}"
                    bat """
                        docker compose -f ${env.COMPOSE_FILE} --env-file ${env.ENV_FILE} down || exit /b 0
                        docker compose -f ${env.COMPOSE_FILE} --env-file ${env.ENV_FILE} up -d --build
                    """
                }
            }
        }

        stage('verificar estado de contenedores') {
            steps {
                dir('Back') {
                    bat 'docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"'
                }
            }
        }
    }

    post {
        success {
            echo "🎉 despliegue completado correctamente para ${env.ENVIRONMENT}"
        }
        failure {
            echo "💥 error durante el despliegue en ${env.ENVIRONMENT}"
        }
    }
}
