pipeline {
    agent any

    environment {
        DOTNET_CLI_HOME = 'C:\\jenkins\\.dotnet'
        DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
        DOTNET_NOLOGO = '1'
    }

    stages {
        stage('Detectar entorno modificado') {
            steps {
                script {
                    echo "🔍 Detectando entorno modificado en el commit..."

                    // Detectar archivos cambiados desde el último commit en main
                    def changedFiles = bat(
                        script: 'git diff --name-only HEAD~1 HEAD',
                        returnStdout: true
                    ).trim().split('\n')

                    def envChanged = ""
                    if (changedFiles.any { it.startsWith('environments/dev/') }) {
                        envChanged = "dev"
                    } else if (changedFiles.any { it.startsWith('environments/qa/') }) {
                        envChanged = "qa"
                    } else if (changedFiles.any { it.startsWith('environments/staging/') }) {
                        envChanged = "staging"
                    } else if (changedFiles.any { it.startsWith('environments/prod/') }) {
                        envChanged = "prod"
                    }

                    if (envChanged == "") {
                        echo "⚠️ No se detectó ningún entorno modificado — no se desplegará nada."
                        currentBuild.result = 'SUCCESS'
                        return
                    }

                    env.ENVIRONMENT = envChanged
                    env.ENV_DIR = "environments/${envChanged}"
                    env.COMPOSE_FILE = "${env.ENV_DIR}/docker-compose.yml"
                    env.ENV_FILE = "${env.ENV_DIR}/.env"

                    echo "✅ Se detectó cambio en el entorno: ${env.ENVIRONMENT}"
                }
            }
        }

        stage('Restaurar dependencias .NET 8') {
            when { expression { env.ENVIRONMENT != null } }
            steps {
                dir('Web') {
                    bat 'dotnet restore'
                }
            }
        }

        stage('Compilar proyecto .NET 8') {
            when { expression { env.ENVIRONMENT != null } }
            steps {
                dir('Web') {
                    bat 'dotnet build --configuration Release'
                }
            }
        }

        stage('Desplegar entorno modificado') {
            when { expression { env.ENVIRONMENT != null } }
            steps {
                echo "🚀 Desplegando entorno ${env.ENVIRONMENT}..."
                bat """
                    docker compose -f ${env.COMPOSE_FILE} --env-file ${env.ENV_FILE} up -d --build
                """
            }
        }

        stage('Verificar contenedores activos') {
            steps {
                bat 'docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"'
            }
        }
    }

    post {
        success {
            echo "🎉 despliegue exitoso (${env.ENVIRONMENT ?: 'ninguno'})"
        }
        failure {
            echo "💥 error durante el despliegue (${env.ENVIRONMENT ?: 'indeterminado'})"
        }
    }
}
