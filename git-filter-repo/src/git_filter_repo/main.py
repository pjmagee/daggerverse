from typing import Annotated, List, Optional

import dagger
from dagger import Doc, dag, function, object_type


@object_type
class GitFilterRepo:
    """
    A collection of functions for working with git repositories using git-filter-repo.
    """

    @function
    async def filter_repo(
        self,
        src: dagger.Directory,
        args: Annotated[
            List[str],
            Doc("Arguments to pass to git-filter-repo command")
        ],
        git_config_name: Annotated[
            Optional[str],
            Doc("Git config user.name to use")
        ] = None,
        git_config_email: Annotated[
            Optional[str],
            Doc("Git config user.email to use")
        ] = None,
    ) -> dagger.Directory:
        """
        Run git-filter-repo on a git repository with the specified arguments.
        
        This function takes a git repository directory, runs git-filter-repo with the 
        provided arguments, and returns the modified repository.
        """
        # Use a container with git and git-filter-repo installed
        container = (
            dag.container()
            .from_("python:3.12-slim")
            .with_exec(["apt-get", "update"])
            .with_exec(["apt-get", "install", "-y", "git"])
            .with_exec(["pip", "install", "git-filter-repo"])
            # Mount the source directory
            .with_directory("/src", src)
            .with_workdir("/src")
        )
        
        # Set git config if provided
        if git_config_name:
            container = container.with_exec(["git", "config", "--global", "user.name", git_config_name])
        if git_config_email:
            container = container.with_exec(["git", "config", "--global", "user.email", git_config_email])
        
        # Run git-filter-repo with the provided arguments
        container = container.with_exec(["git-filter-repo"] + args)
        
        # Return the modified directory
        return container.directory("/src")
    
    @function
    async def version(self) -> str:
        """Return the version of git-filter-repo being used"""
        return await (
            dag.container()
            .from_("python:3.12-slim")
            .with_exec(["apt-get", "update"])
            .with_exec(["apt-get", "install", "-y", "git"])
            .with_exec(["pip", "install", "git-filter-repo"])
            .with_exec(["git-filter-repo", "--version"])
        ).stdout()
