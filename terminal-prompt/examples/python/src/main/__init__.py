"""Example of a Prompt module"""

import dagger
from dagger import dag, function, object_type


@object_type
class Examples:
    """Examples of prompts"""

    @function
    async def prompt_choices(self) -> str:
        """Prompt with choices"""
        result = dag.terminal_prompt().with_choices(["apple", "bear", "orange"]).with_ci(False).execute()  # choices
        outcome = await result.outcome()  # true or false based on user input
        input = await result.input()  # the user input
        return f"Outcome: {outcome}, Input: {input}"

    @function
    async def prompt_input(self, user_input: str, ci: bool) -> str:
        """Prompt with yes/no options"""
        result = dag.terminal_prompt().with_msg("Continue? (y/n)").with_input(user_input).with_ci(ci).execute()
        outcome = await result.outcome()  # true or false based on user input
        input = await result.input()  # the user input
        return f"Outcome: {outcome}, Input: {input}"

    @function
    async def prompt_options(self, user_input: str = "y", ci: bool = True) -> str:
        """Prompt with options"""
        result = dag.terminal_prompt().with_options(
            ci=ci,  # ci mode, disables terminal prompt
            msg="Continue? (y/n)",  # custom message
            input=user_input,  # input from ci pipeline
            match="y",  # regex match of user input
            choices=[]  # choices
        ).execute()

        outcome = await result.outcome()
        input = await result.input()

        return f"Outcome: {outcome}, Input: {input}"
