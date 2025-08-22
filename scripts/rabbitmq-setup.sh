#!/bin/bash
set -e

echo "Starting RabbitMQ..."
rabbitmq-server &

echo "Waiting for RabbitMQ to start..."
until rabbitmqctl status; do
  sleep 5
done

echo "Enabling RabbitMQ required SVC plugins..."

rabbitmq-plugins enable rabbitmq_auth_backend_ldap
rabbitmq-plugins enable rabbitmq_mqtt


echo "Creating default users..."

: ' 
Create Admin and Anonymous user
'
rabbitmqctl add_user "Administrator" "Passw0rd1#"
rabbitmqctl set_user_tags Administrator administrator

rabbitmqctl add_user "anonymous" "dummy-password"
rabbitmqctl clear_password "anonymous"

rabbitmqctl authenticate_user "Administrator" "Passw0rd1"
