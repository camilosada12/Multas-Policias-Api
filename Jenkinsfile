pipeline {
    agent any

    environment {
        DOTNET_CLI_HOME = '/var/jenkins_home/.dotnet'
        DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
        DOTNET_NOLOGO = '1'
        WORKSPACE_DIR = '/workspace/Back'
    }

    stages {
        // =======================================================
        // 1️⃣ detectar entorno por rama
        // =======================================================
        stage('Detectar entorno por rama') {
            steps {
                script {
                    echo "🔍 detectando entorno según la rama..."
                    def branch = env.BRANCH_NAME?.toLowerCase() ?: "develop"

                    switch (branch) {
                        case 'main': env.ENVIRONMENT = 'prod'; break
                        case 'qa': env.ENVIRONMENT = 'qa'; break
                        case 'staging': env.ENVIRONMENT = 'staging'; break
                        default: env.ENVIRONMENT = 'dev'; break
                    }

                    env.ENV_DIR = "${WORKSPACE_DIR}/environments/${env.ENVIRONMENT}"
                    env.COMPOSE_FILE = "${env.ENV_DIR}/docker-compose.yml"
                    env.ENV_FILE = "${env.ENV_DIR}/.env"
                    env.DB_COMPOSE_FILE = "${WORKSPACE_DIR}/DB/docker-compose.yml"

                    echo "✅ rama detectada: ${branch}"
                    echo "📦 entorno asignado: ${env.ENVIRONMENT}"
                    echo "📄 docker-compose API: ${env.COMPOSE_FILE}"
                    echo "📁 archivo .env: ${env.ENV_FILE}"
                    echo "🗄️ docker-compose DB: ${env.DB_COMPOSE_FILE}"
                }
            }
        }

        // =======================================================
        // 2️⃣ restaurar dependencias .net
        // =======================================================
        stage('Restaurar dependencias .NET 8') {
            steps {
                dir("${WORKSPACE_DIR}/Web") {
                    echo "📦 restaurando dependencias..."
                    sh 'dotnet restore'
                }
            }
        }

        // =======================================================
        // 3️⃣ limpiar y compilar proyecto .net
        // =======================================================
        stage('Compilar proyecto .NET 8') {
            steps {
                dir("${WORKSPACE_DIR}/Web") {
                    echo "🧹 limpiando archivos previos..."
                    // limpia los binarios y referencias bloqueadas
                    sh 'dotnet clean'
                    sh 'find ../Entity -type d \\( -name "bin" -o -name "obj" \\) -exec rm -rf {} +'
                    echo "⚙️ compilando proyecto..."
                    sh 'dotnet build --configuration Release'
                }
            }
        }

        // =======================================================
        // 4️⃣ levantar contenedores
        // =======================================================
        stage('Levantar contenedores') {
            steps {
                echo "🗄️ levantando red y contenedores para ${env.ENVIRONMENT}..."
                sh """
                    cd ${WORKSPACE_DIR}
                    docker network create multas_network || echo "🔹 red multas_network ya existe"
                    docker compose -f DB/docker-compose.yml --env-file ${env.ENV_FILE} up -d
                    docker compose -f ${env.COMPOSE_FILE} --env-file ${env.ENV_FILE} up -d --build
                """
            }
        }

        // =======================================================
        // 5️⃣ desplegar api
        // =======================================================
        stage('Desplegar API') {
            steps {
                echo "🚀 desplegando API para entorno ${env.ENVIRONMENT}..."
                sh """
                    cd ${WORKSPACE_DIR}
                    docker compose -f ${env.COMPOSE_FILE} --env-file ${env.ENV_FILE} up -d --build
                """
            }
        }

        // =======================================================
        // 6️⃣ verificar contenedores activos
        // =======================================================
        stage('Verificar contenedores activos') {
            steps {
                echo "🐳 contenedores activos actualmente:"
                sh 'docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"'
            }
        }
    }

    post {
        success { echo "🎉 despliegue exitoso en ${env.ENVIRONMENT}" }
        failure { echo "💥 error durante el despliegue en ${env.ENVIRONMENT}" }
    }
}
