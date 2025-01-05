"""Apache Avro Tools

This module provides a set of functions to work with Apache Avro Tools.

Shorthand avro tools functions provided by this module are:

- idl2schemata: a helper function to Convert an Avro IDL file to Avro schema files in the provided directory
- idl: a helper function to Convert an Avro IDL file to an Avro schema file

For more advanced usage, you can use the avro-tools function to get a container with Apache Avro Tools installed.

- `avro-tools`: Get a container with Apache Avro Tools installed for running other functions

Example:

dagger call --version=1.11.3 avro-tools with-file --path /app/file --source .\avro-file.avdl with-exec --args idl,/app/file,/output file --path /output contents

"""

from typing import Annotated

from dagger import Doc, dag, function, object_type, Directory, Container, File


@object_type
class ApacheAvroTools:
    """A module for Apache Avro Tools"""

    version: Annotated[str, Doc("The version of Apache Avro Tools to use, retrieved from Maven")] = "1.11.3"

    @function()
    async def idl2schemata(
            self,
            file: Annotated[File, Doc("The Avro IDL file to convert to Avro schema files")]) -> Directory:
        """Convert an Avro IDL file to Avro schema files in the provided directory"""

        container = await self.avro_tools()

        return await (container
                      .with_file(path="/app/file", source=file)
                      .with_exec(args=["idl2schemata", "/app/file", "/output"])
                      .directory("/output")).sync()

    @function()
    async def idl(
            self,
            file: Annotated[File, Doc("The Avro IDL file to convert")]) -> File:
        """Convert an Avro IDL file to Avro schema file"""

        container = await self.avro_tools()

        return await (container
                      .with_file(path="/app/file", source=file)
                      .with_exec(args=["idl", "/app/file", "/output"])
                      .file("/output")).sync()

    @function
    async def avro_tools(self) -> Container:
        """Get a container with Apache Avro Tools installed"""

        container = await (
            dag
            .container()
            .from_("maven:latest")
            .with_workdir("/app")
            .with_new_file(path="/app/pom.xml", contents=f"""
            <project xmlns="http://maven.apache.org/POM/4.0.0"
                     xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                     xsi:schemaLocation="http://maven.apache.org/POM/4.0.0 http://maven.apache.org/xsd/maven-4.0.0.xsd">
                <modelVersion>4.0.0</modelVersion>        
                <groupId>dagger.github.com</groupId>
                <artifactId>avro-tools-fetcher</artifactId>
                <version>1.0-SNAPSHOT</version>        
                <dependencies>
                    <dependency>
                        <groupId>org.apache.avro</groupId>
                        <artifactId>avro-tools</artifactId>
                        <version>{self.version}</version>
                        <exclusions>
                            <exclusion>
                                <groupId>*</groupId>
                                <artifactId>*</artifactId>
                            </exclusion>
                        </exclusions>
                    </dependency>
                </dependencies>
            </project>""")
            .with_exec(
                args=[
                    "mvn",
                    "-f",
                    "/app/pom.xml",
                    "dependency:copy-dependencies",
                    "-DoutputDirectory=/app/lib"]).sync())

        return await container.with_entrypoint(["java", "-jar", f"/app/lib/avro-tools-{self.version}.jar"]).sync()
