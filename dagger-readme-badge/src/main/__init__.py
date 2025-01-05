"""Create a shield.io badge for your dagger project.
"""
import dataclasses
import urllib.parse
from typing import Annotated
from rich.console import Console
from rich.markdown import Markdown

import dagger
from dagger import dag, function, object_type, Doc, client


@object_type
class DaggerBadge:
    """Create a shield.io badge for your dagger project."""

    raw_url: Annotated[str, Doc("The URL to the JSON file containing the version information")]

    __segment_prefix = "![Dagger]"
    __img_shield_link = "https://img.shields.io/badge/dynamic/json"
    __json_query = "$.engineVersion"
    __dagger_label_name = "Dagger"
    __dagger_link = "https://github.com/dagger/dagger"
    __logo_data = "data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIGlkPSJDYWxxdWVfNSIgdmlld0JveD0iMCAwIDIyMS4xMDIgMjIxLjEwMiI+PGRlZnM+PHN0eWxlPi5jbHMtNHtmaWxsOiNiZTFkNDN9LmNscy01e2ZpbGw6IzEzMTIyNn0uY2xzLTZ7ZmlsbDojNDBiOWJjfTwvc3R5bGU+PC9kZWZzPjxjaXJjbGUgY3g9IjExMC41NTEiIGN5PSIxMTAuNTUxIiByPSIxMTAuNTI1IiBjbGFzcz0iY2xzLTUiLz48Y2lyY2xlIGN4PSIxMTAuNTUxIiBjeT0iMTEwLjU1MSIgcj0iOTQuODMzIiBjbGFzcz0iY2xzLTUiIHRyYW5zZm9ybT0icm90YXRlKC00NSAxMTAuNTUgMTEwLjU1MSkiLz48cGF0aCBkPSJNMTcuNTQ4IDkxLjk0NGE5NS4yODggOTUuMjg4IDAgMCAwLTEuNjk5IDIzLjYxNGM1LjgyOC4xMDEgMTAuODUxLTEuMDggMTQuMzQxLTQuMTIyIDE3LjMxNi0xNS4wOTMgNTAuMDg4LTEwLjgxMyA1MC4wODgtMTAuODEzcy0xMC4wNjQtMTcuMzczLTYyLjczLTguNjc5eiIgY2xhc3M9ImNscy02Ii8+PHBhdGggZD0iTTI2LjgxOSA2Ni4wMDJjNDkuMDUzLTE1LjA4NyA2MS42MDkgMTAuMTQgNjQuMjM0IDE4LjI4MiAwIDAgMy4wMDMtNDYuMDUyLTM0Ljc3OC01MS41YTk1LjI2NyA5NS4yNjcgMCAwIDAtMjkuNDU2IDMzLjIxOXoiIGNsYXNzPSJjbHMtNCIvPjxwYXRoIGZpbGw9IiNlZjdiMWEiIGQ9Ik0xMTAuMTY3IDcxLjExOHM1LjEzMy0zMi4yODQgMTQuMjI5LTU0LjM5YTk1LjYyOSA5NS42MjkgMCAwIDAtMjguNDE0LjEwNWM5LjA2OSAyMi4xMDQgMTQuMTg1IDU0LjI4NiAxNC4xODUgNTQuMjg2eiIvPjxwYXRoIGQ9Ik0xNjQuNzA0IDMyLjY5OGMtMzguNDU2IDUuMDE3LTM1LjQyMiA1MS41ODYtMzUuNDIyIDUxLjU4NiAyLjY0MS04LjE5MiAxNS4zMzYtMzMuNjg0IDY1LjE1MS0xNy45OTdhOTUuMjggOTUuMjggMCAwIDAtMjkuNzI5LTMzLjU4OXoiIGNsYXNzPSJjbHMtNCIvPjxwYXRoIGQ9Ik0yMDMuNTc5IDkyLjA3NGMtNTMuMzYzLTkuMDAzLTYzLjUyMiA4LjU1LTYzLjUyMiA4LjU1czMyLjc3Mi00LjI4IDUwLjA4OCAxMC44MTNjMy42NDIgMy4xNzQgOC45NTUgNC4zMTkgMTUuMTA5IDQuMDk4YTk1LjMzNyA5NS4zMzcgMCAwIDAtMS42NzUtMjMuNDYxeiIgY2xhc3M9ImNscy02Ii8+PHBhdGggZmlsbD0iI2ZjYzAwOSIgZD0iTTExMC41NTEgMjA1LjM4NGMyLjYzNyAwIDUuMjQ4LS4xMTMgNy44My0uMzI0LTYuMjU3LTEwLjg1Ny02LjYwOC0yNy4zODgtNi42MDgtMzcuNjE0IDAgMCA2LjI3MSAyNy44OTggMTMuOTE3IDM2LjcyOSAyNS41NC00LjA5OCA0Ny42NzItMTguMzkgNjIuMDg3LTM4LjU3NS0yLjY0Ny0yLjUyMy04LjQ0Ny01Ljk3MS0xNi4zNTkgMS4yMDUgMCAwIDMuNDgyLTEzLjc2IDE5LjExNS03LjMyOS0yLjg4Ny03LjY2NS0xMC41MzMtMTIuOTk0LTE5LjM1Mi0xMi4zMTgtOS4zNjkuNzE5LTE2Ljk1NSA4LjQyNi0xNy41NTMgMTcuODAzLS4zMSA0Ljg2OCAxLjIyNCA5LjM1NyAzLjk0NSAxMi44ODQtNC45MTcuOTMxLTkuMTU5IDMuNzQ3LTExLjk2MyA3LjY2OS0uNzM2LTQuNTQyLTQuNjY1LTguMDE0LTkuNDE1LTguMDE0YTkuNDg5IDkuNDg5IDAgMCAwLTUuNzIgMS45MTZjLTIuMTExLTEuNzMxLTQuNDEzLTQuNDMyLTYuNzAxLTcuODY0LTYuMDMzLTkuMDQ5LTguNjQ2LTIyLjgyNi05LjY0My0zMC43Mi0uMjUzLTEuOTk4LTEuOTUtMy40OS0zLjk2NC0zLjQ5cy0zLjcxMSAxLjQ5MS0zLjk2NCAzLjQ5Yy0uOTk3IDcuODk0LTQuMzAxIDIxLjY3MS0xMC4zMzUgMzAuNzItNy42OTYgMTEuNTQ0LTEzLjA4MyAxMi42OTktMTYuMjkgMGwtLjA1OS4wMTRjLTIuNjk3LTkuNTUyLTExLjQ1Ny0xNi41Ni0yMS44NzMtMTYuNTYtOS43MTggMC0xNy45NzUgNi4xMDktMjEuMjI3IDE0LjY4NWE5NS4zMjcgOTUuMzI3IDAgMCAwIDEwLjkwMSAxMS41MzFjMTEuODQ3LTEwLjIzNyAyMS44NjggMy42NjkgMjEuODY4IDMuNjY5LTguNTIxLTMuOTEtMTQuMzUzLTIuODUtMTguMTg2LS41MzggMTEuMzgzIDkuMTk2IDI0LjkzNCAxNS44MTIgMzkuNzY5IDE4Ljk2IDYuMTkxLTcuMTE1IDEyLjE1MS0yMC4zNDUgMTIuMTUxLTIwLjM0NSAwIDQuNDgzLTIuNTggMTQuOTAyLTYuNDAxIDIxLjM4MmE5NS41NzMgOTUuNTczIDAgMCAwIDE0LjAyOSAxLjAzNXoiLz48cGF0aCBmaWxsPSIjZmZmIiBkPSJNMTEzLjIwOCA4MS43OTVhNC4wNzggNC4wNzggMCAwIDAtMy4wNC0xLjM1MiA0LjA4MSA0LjA4MSAwIDAgMC0zLjA0IDEuMzUyYy0yMS40ODIgMjMuNzg5LTI2Ljc1MSA1NS43MjgtMjAuNTc1IDU5Ljk3NCA2LjQ1OSA0LjQ0IDE0LjUzMi0xNC45MzIgMjMuMjExLTE1LjEzM2guODA4YzguNjc4LjIwMiAxNi43NTIgMTkuNTc0IDIzLjIxMSAxNS4xMzMgNi4xNzctNC4yNDcuOTA4LTM2LjE4Ni0yMC41NzUtNTkuOTc0em0tMy4wNCAzMS41NjhhNi4zNyA2LjM3IDAgMSAxIDAtMTIuNzQgNi4zNyA2LjM3IDAgMCAxIDAgMTIuNzR6Ii8+PC9zdmc+"

    def __post_init__(self):
        if not "dagger.json" in self.raw_url:
            raise ValueError("argument must end with or contain 'dagger.json'")

    @staticmethod
    def render_markdown_to_console(content: str):
        console = Console()
        markdown = Markdown(content)
        console.print(markdown)

    @function
    def link(self) -> str:
        """Create a shield.io badge for your dagger project."""

        query_params = {
            "label": self.__dagger_label_name,
            "query": self.__json_query,
            "link": self.__dagger_link,
            "url": self.raw_url,
            "logo": self.__logo_data
        }

        encoded_query_params = {
            k: urllib.parse.quote(str(v).encode('utf-8'), safe='') for k, v in query_params.items()
        }

        badge_url = f"{self.__img_shield_link}?label={encoded_query_params['label']}&query={encoded_query_params['query']}&link={encoded_query_params['link']}&url={encoded_query_params['url']}&logo={encoded_query_params['logo']}"
        return f"{self.__segment_prefix}({badge_url})"

    @function
    async def add_to_readme(self, file: dagger.File) -> dagger.File:
        """Add the badge to the provided README file."""

        markdown_link = self.link()
        markdown = await file.contents()
        markdown = self.add_badge_to_markdown(markdown=markdown, markdown_link=markdown_link)

        self.render_markdown_to_console(markdown)

        return dag.directory().with_new_file("README.md", markdown).file("README.md")

    @staticmethod
    def add_badge_to_markdown(markdown: str, markdown_link: str) -> str:
        lines = markdown.split('\n')
        found_heading = False

        for i, line in enumerate(lines):
            if line.startswith('#') and not found_heading:
                found_heading = True
            elif found_heading and line.strip():
                lines.insert(i, '')  # Add a blank line before the badge
                lines.insert(i + 1, markdown_link)  # Add the badge
                lines.insert(i + 2, '')  # Add a blank line after the badge
                break

        return '\n'.join(lines)
