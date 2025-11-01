pipeline {
    agent any

    environment {
        // solo si luego usas un registry (puedes comentar estas dos líneas)
        REGISTRY = 'turegistrodocker.com'
        IMAGE_NAME = 'miapp-backend'
    }

    stages {
        stage('Preparar entorno') {
            steps {
                script {
                    // detectar entorno según la rama activa
                    if (env.BRANCH_NAME == 'dev') {
                        ENV_FOLDER = 'dev'
                    } else if (env.BRANCH_NAME == 'qa') {
                        ENV_FOLDER = 'qa'
                    } else if (env.BRANCH_NAME == 'staging') {
                        ENV_FOLDER = 'staging'
                    } else if (env.BRANCH_NAME == 'main' || env.BRANCH_NAME == 'prod') {
                        ENV_FOLDER = 'prod'
                    } else {
                        error("⚠️ La rama '${env.BRANCH_NAME}' no está configurada para despliegue automático.")
                    }

                    echo "💡 Entorno detectado: ${ENV_FOLDER}"
                }
            }
        }

        stage('Construir imagen Docker') {
            steps {
                script {
                    echo "🚧 Construyendo contenedor para entorno ${ENV_FOLDER}..."
                    sh """
                        docker compose \
                        -f docker-compose.yml \
                        -f ./environments/${ENV_FOLDER}/docker-compose.override.yml \
                        --env-file ./environments/${ENV_FOLDER}/.env \
                        build
                    """
                }
            }
        }

        // si no tienes registro de imágenes todavía, puedes comentar este bloque
        stage('Subir imagen al registro') {
            when {
                anyOf {
                    branch 'main'
                    branch 'prod'
                    branch 'staging'
                    branch 'qa'
                    branch 'dev'
                }
            }
            steps {
                script {
                    echo "📦 (opcional) Publicando imagen en el registro..."
                    sh """
                        docker tag backend-${ENV_FOLDER} ${REGISTRY}/${IMAGE_NAME}:${ENV_FOLDER} || true
                        docker push ${REGISTRY}/${IMAGE_NAME}:${ENV_FOLDER} || true
                    """
                }
            }
        }

        stage('Desplegar contenedores') {
            steps {
                script {
                    echo "🚀 Desplegando entorno '${ENV_FOLDER}'..."
                    sh """
                        docker compose \
                        -f docker-compose.yml \
                        -f ./environments/${ENV_FOLDER}/docker-compose.override.yml \
                        --env-file ./environments/${ENV_FOLDER}/.env \
                        down || true

                        docker compose \
                        -f docker-compose.yml \
                        -f ./environments/${ENV_FOLDER}/docker-compose.override.yml \
                        --env-file ./environments/${ENV_FOLDER}/.env \
                        up -d
                    """
                }
            }
        }

        stage('Verificar estado de contenedores') {
            steps {
                script {
                    echo "🩺 Listando contenedores activos..."
                    sh 'docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"'
                }
            }
        }
    }

    post {
        success {
            echo "✅ Despliegue completado correctamente para entorno ${ENV_FOLDER}."
        }
        failure {
            echo "❌ Error durante el pipeline. Revisa los logs en Jenkins."
        }
    }
}
