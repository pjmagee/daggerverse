/**
 * A generated module for Examples functions
 *
 * This module has been generated via dagger init and serves as a reference to
 * basic module structure as you get started with Dagger.
 *
 * Two functions have been pre-created. You can modify, delete, or add to them,
 * as needed. They demonstrate usage of arguments and return types using simple
 * echo and grep commands. The functions can be called from the dagger CLI or
 * from one of the SDKs.
 *
 * The first line in this comment block is a short description line and the
 * rest is a long description with more detail on the module's purpose or usage,
 * if appropriate. All modules should have a short description.
 */
import { dag, Container, Directory, object, func } from "@dagger.io/dagger"

@object()
class Examples {

    @func()
    async prompt_choices(): Promise<string> {
      const result = dag.prompt().withChoices(["apple", "bear", "orange"]).withCi(false).execute()
      const outcome = await result.outcome()
      const input = await result.input()
      return `Outcome: ${outcome}, Input: ${input}`
    }

    @func()
    async prompt_input(user_input: string, ci: boolean): Promise<string> {
      const result = dag.prompt().withMsg("Continue? (y/n)").withInput(user_input).withCi(ci).execute()
      const outcome = await result.outcome()
      const input = await result.input()
      return `Outcome: ${outcome}, Input: ${input}`
    }

    @func()
    async prompt_options(user_input: string = "y", ci: boolean = true): Promise<string> {
      const result = dag.prompt().withOptions({
        ci: ci,
        msg: "Continue? (y/n)",
        input: user_input,
        match: "y",
        choices: []
      }).execute()

      const outcome = await result.outcome()
      const input = await result.input()
      return `Outcome: ${outcome}, Input: ${input}`
    }


}
