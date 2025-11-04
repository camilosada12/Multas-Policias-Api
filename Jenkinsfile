pipeline {
    agent any

    environment {
        DOTNET_CLI_HOME = 'C:\\jenkins\\.dotnet'
        DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
        DOTNET_NOLOGO = '1'
    }

    stages {
        stage('Detectar entorno por rama') {
            steps {
                script {
                    echo "🔍 Detectando entorno según la rama..."
                    def branch = env.BRANCH_NAME?.toLowerCase() ?: "develop"

                    switch (branch) {
                        case 'main':
                            env.ENVIRONMENT = 'prod'
                            break
                        case 'qa':
                            env.ENVIRONMENT = 'qa'
                            break
                        case 'staging':
                            env.ENVIRONMENT = 'staging'
                            break
                        default:
                            env.ENVIRONMENT = 'dev'
                            break
                    }

                    env.ENV_DIR = "environments/${env.ENVIRONMENT}"
                    env.COMPOSE_FILE = "${env.ENV_DIR}/docker-compose.yml"
                    env.ENV_FILE = "${env.ENV_DIR}/.env"

                    echo "✅ Rama detectada: ${branch}"
                    echo "📦 Entorno asignado: ${env.ENVIRONMENT}"
                    echo "📄 docker-compose: ${env.COMPOSE_FILE}"
                    echo "📁 archivo .env: ${env.ENV_FILE}"
                }
            }
        }

        stage('Restaurar dependencias .NET 8') {
            steps {
                dir('Web') {
                    echo "📦 Restaurando dependencias..."
                    bat 'dotnet restore'
                }
            }
        }

        stage('Compilar proyecto .NET 8') {
            steps {
                dir('Web') {
                    echo "⚙️ Compilando proyecto..."
                    bat 'dotnet build --configuration Release'
                }
            }
        }

        stage('Desplegar entorno') {
            steps {
                script {
                    echo "🚀 Desplegando entorno ${env.ENVIRONMENT}..."
                    bat """
                        docker compose -f ${env.COMPOSE_FILE} --env-file ${env.ENV_FILE} down || exit /b 0
                        docker compose -f ${env.COMPOSE_FILE} --env-file ${env.ENV_FILE} up -d --build
                    """
                }
            }
        }

        stage('Verificar contenedores activos') {
            steps {
                echo "🐳 Contenedores activos actualmente:"
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
