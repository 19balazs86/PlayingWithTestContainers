#!/bin/bash

# Check if the correct number of arguments are provided
if [ "$#" -lt 2 ]; then
    echo "Usage: $0 <stack-name> <template-file> [parameters]"
    exit 1
fi

STACK_NAME=$1
TEMPLATE_FILE=$2
PARAMETERS=${3:-}

# Validate the template
echo "Validating template..."
aws cloudformation validate-template --template-body "file://$TEMPLATE_FILE"

if [ $? -ne 0 ]; then
    echo "Template validation failed."
    exit 1
fi

# Check if the stack exists
STACK_STATUS=$(aws cloudformation describe-stacks --stack-name "$STACK_NAME" 2>&1)

if [[ $STACK_STATUS == *"does not exist"* ]]; then
    echo "Stack does not exist. Creating stack..."
    if [ -z "$PARAMETERS" ]; then
        aws cloudformation create-stack --stack-name "$STACK_NAME" --template-body "file://$TEMPLATE_FILE"
    else
        aws cloudformation create-stack --stack-name "$STACK_NAME" --template-body "file://$TEMPLATE_FILE" --parameters "$PARAMETERS"
    fi
else
    echo "Stack exists. Updating stack..."
    if [ -z "$PARAMETERS" ]; then
        aws cloudformation update-stack --stack-name "$STACK_NAME" --template-body "file://$TEMPLATE_FILE"
    else
        aws cloudformation update-stack --stack-name "$STACK_NAME" --template-body "file://$TEMPLATE_FILE" --parameters "$PARAMETERS"
    fi
fi
