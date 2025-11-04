pipeline {
    agent any

    environment {
        DOTNET_CLI_HOME = 'C:\\jenkins\\.dotnet'
        DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
        DOTNET_NOLOGO = '1'
    }

    stages {
        // ===============================
        // 1️⃣ Detectar entorno por rama
        // ===============================
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
                    env.DB_COMPOSE_FILE = "docker-compose.db.yml" // Docker Compose de PostgreSQL

                    echo "✅ Rama detectada: ${branch}"
                    echo "📦 Entorno asignado: ${env.ENVIRONMENT}"
                    echo "📄 docker-compose API: ${env.COMPOSE_FILE}"
                    echo "📁 archivo .env: ${env.ENV_FILE}"
                    echo "🗄️ docker-compose DB: ${env.DB_COMPOSE_FILE}"
                }
            }
        }

        // ===============================
        // 2️⃣ Restaurar dependencias .NET
        // ===============================
        stage('Restaurar dependencias .NET 8') {
            steps {
                dir('Web') {
                    echo "📦 Restaurando dependencias..."
                    bat 'dotnet restore'
                }
            }
        }

        // ===============================
        // 3️⃣ Compilar proyecto .NET
        // ===============================
        stage('Compilar proyecto .NET 8') {
            steps {
                dir('Web') {
                    echo "⚙️ Compilando proyecto..."
                    bat 'dotnet build --configuration Release'
                }
            }
        }

        // ===============================
        // 4️⃣ Levantar bases de datos (PostgreSQL)
        // ===============================
        stage('Preparar red y bases de datos') {
            steps {
                echo "🗄️ Levantando bases de datos (Postgres)..."
                bat """
                    docker network create multas_network || echo "🔹 Red multas_network ya existe"
                    docker compose -f ${env.DB_COMPOSE_FILE} up -d
                """
            }
        }

        // ===============================
        // 5️⃣ Desplegar API + SQL Server
        // ===============================
        stage('Desplegar entorno') {
            steps {
                script {
                    echo "🚀 Desplegando API + SQL Server en entorno ${env.ENVIRONMENT}..."
                    bat """
                        docker compose -f ${env.COMPOSE_FILE} --env-file ${env.ENV_FILE} down || exit /b 0
                        docker compose -f ${env.COMPOSE_FILE} --env-file ${env.ENV_FILE} up -d --build
                    """
                }
            }
        }

        // ===============================
        // 6️⃣ Verificar contenedores activos
        // ===============================
        stage('Verificar contenedores activos') {
            steps {
                echo "🐳 Contenedores activos actualmente:"
                bat 'docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"'
            }
        }
    }

    post {
        success {
            echo "🎉 Despliegue exitoso en ${env.ENVIRONMENT}"
        }
        failure {
            echo "💥 Error durante el despliegue en ${env.ENVIRONMENT}"
        }
    }
}
