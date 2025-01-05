"""Uses the dagger Terminal to prompt the user for input."""

import dataclasses
import time
import re
import dagger
from dagger import object_type, function, dag, field


@object_type
class Options:
    ci: bool = dataclasses.field(default=True)
    msg: str = dataclasses.field(default="Continue? (y/n)")
    input: str = dataclasses.field(default="n")
    match: str = dataclasses.field(default="y")
    choices: list[str] = dataclasses.field(default_factory=lambda: [])


@object_type
class Result:
    
    outcome: bool = field()

    
    input: str = field()


@object_type
class TerminalPrompt:

    options: Options = dataclasses.field(init=False, default_factory=lambda: Options())

    @function
    async def with_options(self,
                           ci: bool = True,
                           msg: str = "Continue? (y/n)",
                           input: str = "",
                           match: str = "y",
                           choices: list[str] = ()) -> "TerminalPrompt":
        """Sets the options for the prompt."""
        self.options = Options(
            ci=ci,
            msg=msg,
            input=input,
            match=match,
            choices=choices)
        return self

    @function
    async def with_ci(self, ci: bool) -> "TerminalPrompt":
        """Sets the ci flag. If true, the prompt will not wait for user input."""
        self.options.ci = ci
        return self

    @function
    async def with_msg(self, msg: str) -> "TerminalPrompt":
        """Sets the message to display to the user."""
        self.options.msg = msg
        return self

    @function
    async def with_input(self, input: str) -> "TerminalPrompt":
        """Sets the input to return if the ci flag is set to true."""
        self.options.input = input
        return self

    @function
    async def with_match(self, match: str) -> "TerminalPrompt":
        """Sets the regex pattern that the user input must match."""
        self.options.match = match
        return self

    @function
    async def with_choices(self, choices: list[str]) -> "TerminalPrompt":
        """Sets the list of choices the user can select from."""
        self.options.choices = choices
        return self

    @function
    async def execute(self) -> Result:
        """Executes the prompt and returns the result."""
        if self.options.ci:
            if len(self.options.choices) > 0:
                return Result(
                    outcome=self.options.input in self.options.choices,
                    input=self.options.input)
            else:
                return Result(
                    outcome=re.search(pattern=self.options.match, string=self.options.input) is not None,
                    input=self.options.input)
        else:
            if len(self.options.choices) > 0:
                return await self.user_choice_reply
            else:
                return await self.user_text_reply()

    @property
    async def user_choice_reply(self):
        choices_str: str = " ".join(f"\"{choice}\"" for i, choice in enumerate(self.options.choices))
        script = f"""
                    #!/bin/sh
                    choices=({choices_str})
                    echo "{self.options.msg} (^C to abort)"
                    select choice in "${{choices[@]}}"; do
                        # choice being empty signals invalid input.
                        [[ -n $choice ]] || {{ echo "Invalid choice. Please try again." >&2; continue; }}
                        break # a valid choice was made, exit the prompt.
                    done
                    echo $choice > /tmp/prompt/input
                    """
        cache_buster = f"{time.time()}"
        cache = dag.cache_volume(key=cache_buster)

        response = await (dag.container()
                          .from_("bash")
                          .with_mounted_cache(path="/tmp/prompt", cache=cache)
                          .terminal(cmd=["bash", "-c", script])
                          .with_exec(["sh", "-c", f": {cache_buster} && exit 0"])
                          .with_exec(["cat", "/tmp/prompt/input"])
                          .stdout())

        return Result(
            outcome=self.options.choices.index(response.strip()) != -1,
            input=response)

    async def user_text_reply(self):
        cache_buster = f"{time.time()}"
        response: str = (await (dag
                                .container()
                                .from_("bash")
                                .with_mounted_cache("/tmp/prompt", dag.cache_volume(key=f"{time.time()}"))
                                .terminal(cmd=["sh", "-c", f"read -p '{self.options.msg} ' && echo $REPLY > /tmp/prompt/input"])
                                .with_exec(["sh", "-c", f": {cache_buster} && exit 0"])
                                .with_exec(["cat", "/tmp/prompt/input"])
                                .stdout())).strip()
        return Result(
            outcome=re.search(
                pattern=self.options.match,
                string=response) is not None,
            input=response)
