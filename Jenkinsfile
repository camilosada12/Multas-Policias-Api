/// <summary>
/// Jenkinsfile principal para despliegue automatizado del proyecto MULTAS.
/// Este pipeline detecta el entorno desde Back/.env,
/// compila el proyecto .NET 9 y ejecuta el docker-compose correspondiente dentro de Back/environments/{entorno}.
/// </summary>

pipeline {
    agent any

    environment {
        DOTNET_CLI_HOME = 'C:\\jenkins\\.dotnet'
        DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
        DOTNET_NOLOGO = '1'
    }

    stages {

        stage('leer entorno desde Back/.env') {
            steps {
                dir('Back') {
                    script {
                        // ✅ leemos el ENVIRONMENT desde Back/.env
                        def envValue = powershell(
                            script: "(Get-Content .env | Where-Object { \$_ -match '^ENVIRONMENT=' }) -replace '^ENVIRONMENT=', ''",
                            returnStdout: true
                        ).trim()

                        if (!envValue) {
                            error "❌ no se encontró ENVIRONMENT en Back/.env"
                        }

                        env.ENVIRONMENT = envValue.toLowerCase()
                        env.ENV_DIR = "environments/${env.ENVIRONMENT}"
                        env.COMPOSE_FILE = "${env.ENV_DIR}/docker-compose.yml"
                        env.ENV_FILE = "${env.ENV_DIR}/.env"

                        echo "✅ entorno detectado: ${env.ENVIRONMENT}"
                        echo "📄 archivo compose: ${env.COMPOSE_FILE}"
                        echo "📁 archivo de entorno: ${env.ENV_FILE}"
                    }
                }
            }
        }

        stage('restaurar dependencias .NET') {
            steps {
                dir('Back/Web') {
                    bat '''
                        echo 🧩 restaurando dependencias .NET...
                        dotnet restore Web.csproj
                    '''
                }
            }
        }

        stage('compilar proyecto .NET') {
            steps {
                dir('Back/Web') {
                    echo '⚙️ compilando Web.csproj...'
                    bat 'dotnet build Web.csproj --configuration Release'
                }
            }
        }

        stage('publicar y construir imagen Docker') {
            steps {
                dir('Back') {
                    echo "🐳 construyendo imagen Docker: multas-api-${env.ENVIRONMENT}..."
                    bat "docker build -t multas-api-${env.ENVIRONMENT}:latest -f Dockerfile ."
                }
            }
        }

        stage('desplegar API con Docker Compose') {
            steps {
                dir('Back') {
                    echo "🚀 desplegando API en entorno: ${env.ENVIRONMENT}"
                    bat """
                        docker compose -f ${env.COMPOSE_FILE} --env-file ${env.ENV_FILE} down || exit /b 0
                        docker compose -f ${env.COMPOSE_FILE} --env-file ${env.ENV_FILE} up -d --build
                    """
                }
            }
        }

        stage('verificar estado de contenedores') {
            steps {
                bat 'docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"'
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
