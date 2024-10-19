#!/bin/bash

# Default values
STACK_NAME="RDS-Postgres"
TEMPLATE_FILE="AWS-RDS-DBInstance-template.yml"

./AWS-Run-template.sh "$STACK_NAME" "$TEMPLATE_FILE"
