#!/bin/bash

set -e
set -u

function create_database(){
  local database=$l
  echo " Creating database '$database' "
  psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" <<-EOSQL
    CREATE USER $database;
  	CREATE DATABASE $database;
  	GRANT ALL PRIVILEGES ON DATABASE docker TO $database;
  EOSQL
}


if [ -n "POSTGRES_MULTIPLE_DATABASES" ]; then
  echo "Creating multiple databases: $POSTGRES_MULTIPLE_DATABASES"
  for db in $(echo $POSTGRES_MULTIPLE_DATABASES | tr "," " "); do
    create_database $db
  done
  echo "Databases created!"
fi