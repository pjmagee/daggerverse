# Apache Avro Tools Module

This Python module provides a set of functions to work with Apache Avro Tools. It is designed to simplify the process of converting Avro IDL files to Avro schema files and running other functions with Apache Avro Tools.

## Features

- `idl2schemata`: Convert an Avro IDL file to Avro schema files in the provided directory.  
- `idl`: Convert an Avro IDL file to an Avro schema file.  
- `avro-tools`: Get a container with Apache Avro Tools installed for running other functions.  

## Dependencies

This module uses the following dependencies:

- Apache Avro Tools (retrieved from Maven)  
- Python 3.10 or later  
- Dagger  

## Avro tools version

The default version of Apache Avro Tools used is 1.11.3. You can specify a different version by using `dagger call --version=1.11.3 avro-tools`
