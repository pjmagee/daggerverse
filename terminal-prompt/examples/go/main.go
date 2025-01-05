// A generated module for Examples functions
//
// This module has been generated via dagger init and serves as a reference to
// basic module structure as you get started with Dagger.
//
// Two functions have been pre-created. You can modify, delete, or add to them,
// as needed. They demonstrate usage of arguments and return types using simple
// echo and grep commands. The functions can be called from the dagger CLI or
// from one of the SDKs.
//
// The first line in this comment block is a short description line and the
// rest is a long description with more detail on the module's purpose or usage,
// if appropriate. All modules should have a short description.

package main

import (
	"context"
	"strconv"
)

// Examples is a struct that contains the functions for the Examples module
type Examples struct{}

// Prompt_Choice is a function that demonstrates how to use the Prompt() function
func (m *Examples) Prompt_Choice(userInput string) string {
	result := dag.Prompt().
		WithChoices([]string{"Option 1", "Option 2", "Option 3"}). // A list of custom choices
		WithMsg("Select an option").                               // A custom message for the prompt
		WithInput(userInput).                                      // pass in from the ci pipeline
		WithCi(true).                                              // disabled ci mode will open a terminal prompt
		Execute()
	outcome, _ := result.Outcome(context.Background()) // true or false if input was a valid choice
	input, _ := result.Input(context.Background())     // The selected choice
	return "Outcome: " + strconv.FormatBool(outcome) + ", Input: " + input
}

// Prompt_Options is a function that demonstrates how to use the Prompt() function
func (m *Examples) Prompt_Options(userInput string, ci bool) string {
	result := dag.Prompt().WithOptions(ci, "Continue? (y/n)", userInput, "y", []string{}).Execute()
	outcome, _ := result.Outcome(context.Background()) // true or false if input was a valid choice
	input, _ := result.Input(context.Background())     // The selected choice
	return "Outcome: " + strconv.FormatBool(outcome) + ", Input: " + input
}

// Prompt_Input is a function that demonstrates how to use the Prompt() function
func (m *Examples) Prompt_Input(userInput string, ci bool) string {
	result := dag.Prompt().
		WithMsg("Do you want to continue? (y/n)"). // A custom message for the prompt
		WithInput(userInput).                      // pass in from the ci pipeline
		WithMatch("y").                            // A custom regex match for the user input
		WithCi(ci).                                // disabled ci mode will open a terminal prompt
		Execute()
	outcome, _ := result.Outcome(context.Background()) // true or false if input matched the regex
	input, _ := result.Input(context.Background())     // The user input
	return "Outcome: " + strconv.FormatBool(outcome) + ", Input: " + input
}
