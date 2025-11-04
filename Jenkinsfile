pipeline {
    agent any

    environment {
        DOTNET_CLI_HOME = 'C:\\jenkins\\.dotnet'
        DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
        DOTNET_NOLOGO = '1'
    }

    stages {
        stage('Leer entorno desde Api/.env') {
            steps {
                dir('Web') {  // carpeta donde está tu API
                    script {
                        def envValue = powershell(
                            script: "(Get-Content .env | Where-Object { \$_ -match '^ENVIRONMENT=' }) -replace '^ENVIRONMENT=', ''",
                            returnStdout: true
                        ).trim()

                        if (!envValue) {
                            error "❌ No se encontró ENVIRONMENT en Web/.env"
                        }

                        env.ENVIRONMENT = envValue
                        env.ENV_DIR = "environments/${env.ENVIRONMENT}"
                        env.COMPOSE_FILE = "${env.ENV_DIR}/docker-compose.yml"
                        env.ENV_FILE = "${env.ENV_DIR}/.env"
                        env.DB_COMPOSE_FILE = "DB/docker-compose.yml"

                        echo """
                        ✅ Entorno detectado: ${env.ENVIRONMENT}
                        📄 Compose API: ${env.COMPOSE_FILE}
                        📁 Env file: ${env.ENV_FILE}
                        🗄️ Compose DB: ${env.DB_COMPOSE_FILE}
                        """
                    }
                }
            }
        }

        // 2️⃣ Restaurar dependencias
        stage('Restaurar dependencias .NET') {
            steps {
                dir('Web') {
                    bat '''
                        echo 🔧 Restaurando dependencias .NET...
                        dotnet restore
                    '''
                }
            }
        }

        // 3️⃣ Compilar proyecto
        stage('Compilar proyecto .NET') {
            steps {
                dir('Web') {
                    bat 'dotnet build --configuration Release --no-restore'
                }
            }
        }

        // 4️⃣ Preparar red y levantar bases de datos
        stage('Preparar red y base de datos') {
            steps {
                bat """
                    echo 🌐 Creando red externa compartida (si no existe)...
                    docker network create multas_network || echo "🔹 Red existente"

                    echo 🗄️ Levantando bases de datos SQL Server...
                    docker compose -f ${env.DB_COMPOSE_FILE} up -d
                """
            }
        }

        // 5️⃣ Desplegar API
        stage('Desplegar API') {
            steps {
                bat """
                    echo 🚀 Desplegando API (${env.ENVIRONMENT})...
                    docker compose -f ${env.COMPOSE_FILE} --env-file ${env.ENV_FILE} up -d --build
                """
            }
        }

        // 6️⃣ Verificar contenedores
        stage('Verificar contenedores activos') {
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
