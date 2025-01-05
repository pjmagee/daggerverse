# Create prompts

Prompts the user for input when run locally, and can be passed values from CI in non-interactive mode without a tty

## Options

```bash
dagger call with-ci --ci=false \
            with-msg --msg "custom msg: y/n?" 
            options
```

```txt
✔ connect 1.1s
✔ initialize 9.9s
✔ prepare 0.0s
✔ prompt: Prompt! 1.1s
✔ Prompt.withCi(ci: false): Prompt! 0.8s
✔ Prompt.withMsg(msg: "custom msg: y/n?"): Prompt! 0.8s
✔ Prompt.options: PromptOptions! 0.0s
✔ PromptOptions.ci: Boolean! 0.0s
✔ PromptOptions.input: String! 0.0s
✔ PromptOptions.choices: [String!]! 0.0s
✔ PromptOptions.msg: String! 0.0s
✔ PromptOptions.match: String! 0.0s

_type: PromptOptions
choices: []
ci: false
input: "n"
match: "y"
msg: 'custom msg: y/n?'
```

## Choices

```bash
dagger call with-ci --ci=true \
            with-choices --choices=apple,pear,banana \
            with-input --input=apple \
            with-msg --msg="custom msg: y/n?" \
            options
```

```txt
✔ connect 1.2s
✔ initialize 1.8s
✔ prepare 0.0s
✔ prompt: Prompt! 0.9s
✔ Prompt.withCi(ci: true): Prompt! 0.7s
✔ Prompt.withChoices(choices: ["apple", "pear", "banana"]): Prompt! 0.7s
✔ Prompt.withInput(input: "apple"): Prompt! 0.7s
✔ Prompt.withMsg(msg: "custom msg: y/n?"): Prompt! 0.7s
✔ Prompt.execute: PromptResult! 0.7s
✔ PromptResult.outcome: Boolean! 0.0s
✔ PromptResult.input: String! 0.0s

_type: PromptResult
input: apple
outcome: true
```

## Choices Terminal

Use `with-ci=false` for terminal mode.

```txt
dagger call with-ci --ci=false
            with-choices --choices=apple,pear,banana \
            with-input --input=apple \
            with-msg --msg="custom msg: y/n?" \
            execute

● Attaching terminal:
    container: Container!
    Container.from(address: "bash"): Container!    
    Container.withMountedCache(
        cache: cacheVolume(key: "1725832140.0709038"): CacheVolume!
        path: "/tmp/prompt"
      ): Container!

custom msg: y/n? (^C to abort)
1) apple
2) pear
3) banana
#? <-- User input here
```


## Input

```bash
dagger call with-ci --ci=false \
            with-msg --msg="enter 'approve' to continue" \
            with-match --match="approve" \
            execute
```

## Prompt CI mode

```bash
input='y'
dagger call with-ci --ci=true with-input --input=$input execute
```
